# Architecture

MOS 6502 emulator implemented in .NET 10 (C# 14).

The architecture organizes emulator responsibilities across Domain, Application, and Infrastructure layers, prioritizing correctness, simplicity, locality, and low coupling over unnecessary indirection or framework abstractions.

## Core Principles

### Specification First

The 6502 hardware specification is the single source of truth.

Implementation decisions must preserve the observable behavior defined by the specification.

When the implementation and specification disagree, the implementation is incorrect.

### Simplicity First

Do not introduce abstractions until the implementation demonstrates a genuine need for one.

Prefer:

- Direct method calls
- Concrete types
- Explicit constructor dependencies
- Small cohesive components
- Local, visible state
- Simple fixed-width data structures
- Explicit control flow

Avoid:

- Unnecessary interfaces
- Dependency injection frameworks / IoC containers
- Factories and service locators
- Generic abstractions
- Deep inheritance hierarchies
- Wrapper types without meaningful domain behavior
- Indirection created solely for organizational symmetry

### Low Coupling

Components depend only on the explicit collaborators they need to execute their responsibilities.

- `Bus` does not depend on `Cpu` or opcode logic.
- `Cpu` represents pure register and status flag state.
- `Addressing` depends only on `Cpu` (for index registers) and `Bus` (for indirect address fetching).
- `Instructions` depends on `Cpu` (for accumulator, registers, and flags) and `Bus` (for memory reads, writes, and stack storage).
- `Emulator` orchestrates these domain components into a complete runnable processor.

### Single Source of Truth

- Register and flag state belongs exclusively to `Cpu`.
- Addressable memory belongs exclusively to `Bus`.
- Domain operations operate on these real instances directly without duplicating state.

---

## Layer Responsibilities

### Solution & Project Structure

The codebase is organized into a single solution (`mos6502.slnx`) containing two projects:

1. **`src/mos6502.csproj`**: Core emulator library and CLI functional test runner (`net10.0`).
2. **`tests/mos6502.Tests/mos6502.Tests.csproj`**: Unit, integration, and functional test suite mirroring `src/`.

```
src/
├── Domain/
│   ├── Entities/
│   │   ├── Addressing.cs       # 6502 addressing mode calculations
│   │   ├── Bus.cs              # 64 KiB addressable memory bus
│   │   ├── Cpu.cs              # 6502 registers, stack pointer, and status state
│   │   └── Instructions.cs     # 6502 ALU, stack, branching, and memory instructions
│   ├── Enums/
│   │   └── Status.cs           # Flags (Carry, Zero, Interrupt, Decimal, Break, Overflow, Negative)
│   └── Objects/
│       ├── AddressingResult.cs # Result container for effective address and addressing cycles
│       └── InstructionResult.cs# Result container for base instruction cycles
├── Application/
│   └── Emulator.cs             # Application orchestrator: ROM loader, reset, Step() dispatch, cycle tally
└── Infrastructure/
    └── Cli/
        ├── Program.cs          # Console host for automated test ROM execution
        └── assets/roms/        # Embedded / bundled test ROMs (e.g., nestest.nes)
```

---

## Domain Responsibilities

### Memory Bus (`Mos6502.Domain.Entities.Bus`)

The memory bus represents the physical 64 KiB address space ($0000 to $FFFF).

Responsibilities:
- Stores flat 64 KiB RAM (`Ram[0x10000]`).
- Provides byte-level access (`ReadByte`, `WriteByte`).
- Provides 16-bit little-endian word reads (`ReadWord`).
- Contains no CPU instruction semantics or cycle accounting.

### CPU State (`Mos6502.Domain.Entities.Cpu`)

The CPU represents the internal register and flag state of the MOS 6502.

Owned state:
- `A` (Accumulator, 8-bit)
- `X` (Index Register X, 8-bit)
- `Y` (Index Register Y, 8-bit)
- `StackPointer` (Stack Pointer, 8-bit)
- `PC` (Program Counter, 16-bit)
- `Status` (Processor Status Flags, `Status` enum)

Methods:
- `Reset(u16 resetVectorAddress)`: Initializes registers and default reset flags (`StackPointer = 0xFD`, `Status = Status.Interrupt`, `PC = resetVectorAddress`).

### Addressing Modes (`Mos6502.Domain.Entities.Addressing`)

Calculates effective addresses and operand values according to 6502 addressing modes.

Constructor dependencies: `Addressing(Cpu cpu, Bus bus)`

Supported addressing modes:
- `Absolute`
- `AbsoluteX` (with page crossing penalty detection)
- `AbsoluteY` (with page crossing penalty detection)
- `Immediate`
- `IndexedIndirect` (Zero Page `(indirect, X)`)
- `Indirect` (JMP indirect with hardware page-boundary wrap behavior)
- `IndirectIndexed` (Zero Page `(indirect), Y` with page crossing penalty detection)
- `Relative` (branch target offset)
- `ZeroPage`
- `ZeroPageX`
- `ZeroPageY`

### Instructions (`Mos6502.Domain.Entities.Instructions`)

Implements the official 6502 instruction execution logic, ALU calculations, and status flag updates.

Constructor dependencies: `Instructions(Cpu cpu, Bus bus)`

Responsibilities:
- Arithmetic and logical operations (`ADC`, `SBC`, `AND`, `ORA`, `EOR`, `BIT`, `CMP`, `CPX`, `CPY`).
- Increments and decrements (`INC`, `DEC`, `INX`, `INY`, `DEX`, `DEY`).
- Shifts and rotates (`ASL`, `LSR`, `ROL`, `ROR`).
- Control flow and branches (`BCC`, `BCS`, `BEQ`, `BMI`, `BNE`, `BPL`, `BVC`, `BVS`, `JMP`, `JSR`, `RTS`, `RTI`, `BRK`).
- Stack operations (`PHA`, `PHP`, `PLA`, `PLP`).
- Status flag clears and sets (`CLC`, `SEC`, `CLI`, `SEI`, `CLV`, `CLD`, `SED`).
- Register transfers (`TAX`, `TXA`, `TAY`, `TYA`, `TSX`, `TXS`).
- Loads and stores (`LDA`, `LDX`, `LDY`, `STA`, `STX`, `STY`).

---

## Application Responsibilities

### Emulator Service (`Mos6502.Application.Emulator`)

The `Emulator` coordinates the domain entities and exposes the execution interface.

Constructor: `Emulator(Bus bus)` creates internal `Cpu`, `Addressing`, and `Instructions` instances.

Responsibilities:
- Program loading via `LoadRom(byte[] rom, u16 startAddress)`.
- Reset coordination (`Reset()`).
- Step execution loop (`Step()`): fetches the next opcode at `PC`, decodes through an explicit `switch` expression, delegates to the appropriate instruction-mode execution method, advances `PC`, and returns total elapsed cycles for the step.
- Named instruction helper methods (e.g., `AdcImmediate`, `StaAbsoluteX`, `JmpIndirect`) that assemble the addressing calculation with the instruction execution and return exact cycle counts.

---

## Infrastructure Responsibilities

### CLI Test Runner (`Mos6502.Infrastructure.Cli.Program`)

The CLI host executes external test ROMs (such as `nestest.nes`) against the emulator.

Responsibilities:
- Locates and parses binary test ROMs.
- Initializes the `Bus` and `Emulator`.
- Steps execution until completion or error condition.
- Verifies output against hardware test vectors ($0002 status register in `nestest`).

---

## Dependency Rules

The dependency flow strictly follows:

```
Infrastructure (Cli)
        ↓
   Application (Emulator)
        ↓
     Domain (Bus, Cpu, Addressing, Instructions)
```

1. **Domain** has zero dependencies on Application, Infrastructure, or third-party packages. It relies exclusively on .NET standard primitives and `GlobalUsings`.
2. **Application** coordinates Domain entities via direct constructor dependency passing.
3. **Infrastructure** consumes Application services to run programs.

---

## Testing Strategy Alignment

Tests mirror the `src/` directory layout:

- `tests/mos6502.Tests/Domain/Entities/BusEntity/`: Memory access and bounds verification.
- `tests/mos6502.Tests/Domain/Entities/CpuEntity/`: Register state and reset verification.
- `tests/mos6502.Tests/Domain/Entities/AddressingEntity/`: Addressing calculations and page boundary cycle rules.
- `tests/mos6502.Tests/Domain/Entities/InstructionsEntity/`: Opcode semantics and flag modifications.
- `tests/mos6502.Tests/Application/Emulator/`: Opcode dispatch pipeline and multi-step execution workflows (`Functional/`).

---

## Fixed-Width Types

Fixed-width types represent actual 6502 hardware widths:

- `u8` (`byte`) for 8-bit registers, flags, memory bytes, and opcodes.
- `i8` (`sbyte`) for signed 8-bit branch offsets.
- `u16` (`ushort`) for 16-bit addresses and pointers.
- `cycle` (`int`) for cycle count bookkeeping.

Type aliases are globally declared in `src/GlobalUsings.cs` and `tests/mos6502.Tests/GlobalUsings.cs`.
