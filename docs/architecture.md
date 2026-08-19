# Architecture

MOS 6502 Emulator implemented in .NET 9 (C# 13) following Domain-Driven Design (DDD) principles.

## Layer Structure & Ownership

```
src/
├── Application/
│   └── Emulator.cs             # Application service: cycle orchestration, program loading & reset
└── Domain/
    ├── Entities/
    │   ├── Bus.cs              # 64 KiB flat RAM & memory read/write operations
    │   └── Cpu.cs              # Core CPU state (registers, flags), addressing resolution & instructions
    ├── Enums/
    │   ├── AddressingModes.cs  # 6502 addressing mode definitions
    │   └── Status.cs           # Processor status flags bitmask (C, Z, I, D, B, V, N)
    └── Objects/
        ├── Instruction.cs      # Instruction value object (Opcode, Operand)
        ├── Unassigned8Bits.cs  # Fixed-width 8-bit value type wrapper with arithmetic/bitwise operators
        └── Unassigned16Bits.cs # Fixed-width 16-bit address value type wrapper

tests/
└── Domain.Tests/
    ├── AddresingModeTests.cs   # Unit tests for 6502 addressing mode operand resolution
    ├── CpuExtensions.cs        # Internal test helpers for CPU test routines
    ├── InstructionTests.cs     # Unit tests for CPU instructions and flag calculations
    ├── MemoryTests.cs          # Unit tests for Bus 64 KiB memory read/write operations
    ├── RegistersTests.cs       # Unit tests for CPU registers & status flag state transitions
    └── Domain.Tests.csproj     # Test project definition (.NET 9, xUnit, FluentAssertions)
```

### Future / Planned Expansions (as implementation scales)
- `Domain/Interfaces/` — `IBus` or `IMemoryMappedDevice` abstractions for future IO/cartridge mapping.
- `Application/Services/` or instruction modules — Further partitioning opcode decoding/dispatching as the full instruction set (56 instructions, 151 official opcodes) is populated.

## Module Responsibilities & Dependency Rules

1. **Application Layer (`Emulator`)**:
   - Acts as the application service/orchestrator.
   - Coordinates the Fetch-Decode-Execute pipeline between `Bus` and `Cpu`.
   - Manages high-level lifecycle methods (`Reset()`, `LoadProgram()`, `Step()`).
   - Depends only on Domain types (`Entities`, `Enums`, `Objects`).

2. **Domain Layer (`Cpu`, `Bus`, Value Objects)**:
   - **`Cpu`**: Owns all internal processor registers (`A`, `X`, `Y`, `PC`, `SP`, `Flags`), arithmetic/flag mutation logic, and operand resolution per addressing mode.
   - **`Bus`**: Pure memory storage and address indexing with zero dependency on CPU instruction logic or application workflow.
   - **Value Objects / Enums**: Immutable record structs (`Unassigned8Bits`, `Unassigned16Bits`, `Instruction`) providing type-safe fixed-width numeric boundaries.
   - Domain layer has **zero external third-party dependencies** and zero awareness of CLI/Application hosting.

3. **Test Suite (`Domain.Tests`)**:
   - Mirrors domain types and directly tests addressing modes, CPU arithmetic/flags, memory read/write, and register behaviors.
   - Depends on `mos6502` project and xUnit / FluentAssertions testing frameworks.

## Data Flow (Fetch-Decode-Execute)

1. **Fetch:** `Emulator.Step()` reads the opcode byte from `Bus` at the memory address pointed to by `Cpu.ProgramCounter`.
2. **Decode:** `Emulator` resolves the required addressing mode and instruction byte length, fetching raw operand bytes if applicable.
3. **Execute:** `Emulator` dispatches execution to the corresponding `Cpu` instruction method (e.g. `Cpu.ADC()`), where `Cpu` resolves effective operand data via addressing mode logic and computes arithmetic/flags.
4. **Advance:** `Cpu.AdvancePC(length)` increments the Program Counter to the next instruction.

## Non-Negotiable Constraints

- **Pure .NET 9 Standard Library**: Domain logic must remain free of third-party library dependencies.
- **Single Source of Truth**: All CPU state (registers, flags) resides strictly within `Cpu`.
- **Explicit Fixed-Width Types**: Memory addresses and byte values must consistently use `Unassigned8Bits`/`Unassigned16Bits` (or explicit `byte`/`ushort`).
- **Hardware-Accurate Specifications**: Instruction execution and flag behaviors must strictly adhere to the 6502 hardware specification in `docs/hardware-specification/`.
