# Architecture

MOS 6502 Emulator implemented in .NET 9 (C# 13).

## Directory Structure & Module Ownership

- `Program.cs` — Console entry point, runner, or interactive CLI host.
- `src/` — Main emulator domain logic (to be created as implementation scales).
  - `src/Cpu/` — Core MOS 6502 CPU state, registers, execution loop, and opcode dispatching.
  - `src/Memory/` — Bus, RAM, ROM mapping, and memory-mapped IO interfaces.
  - `src/Addressing/` — Addressing mode resolution logic.
  - `src/Opcodes/` — Opcode implementations and instruction decoders.
- `tests/` — Unit and integration test suite.
- `docs/` — Specifications and AI agent harness context.
  - `docs/specs/` — MOS 6502 hardware specs (registers, opcodes, addressing modes).

## Dependency Rules

- Low-level domain logic (`Memory`, `Addressing`, `Cpu`) must have zero dependencies on console CLI or external IO UI layers.
- Memory modules must remain agnostic of CPU instruction dispatch logic (CPU accesses Memory via a memory bus interface/abstraction).
- Instruction implementations in `Opcodes` must manipulate CPU state via explicit CPU register/flag methods or properties, never via direct side effects on external system state.

## Data Flow

1. **Clock Cycle / Instruction Fetch:** CPU reads opcode byte from bus at memory location defined by Program Counter (`PC`).
2. **Decode & Address Calculation:** CPU resolves target memory address using the opcode's specified `AddressingMode`.
3. **Execute & Commit State:** Opcode handler performs arithmetic/logic operation, updates registers (`A`, `X`, `Y`, `SP`, `P`), increments `PC`, and advances total tick/cycle count.

## Structural Constraints

- **Single Source of Truth for CPU State:** CPU state (registers, flags, cycle count) resides strictly within the CPU module.
- **No external third-party dependencies** for core CPU emulation logic; keep standard library/runtime pure .NET 9.
