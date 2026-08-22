---
title: Technical Reference Overview
description: Specification of 6502 status register flags, 13 addressing modes, and complete instruction opcode matrix.
---

The Technical Reference quadrant details the exact technical specifications of the MOS 6502 architecture.

## Reference Categories

- **[Complete Instruction Set Matrix](./instruction-matrix)**  
  Full 16x16 table of all 256 opcodes, valid operations, bytes, cycles, and addressing modes.

- **[Status Register & Flags](./status-flags)**  
  Bit allocation, condition meanings, and instruction side effects on flags (N, V, U, B, D, I, Z, C).

- **[Addressing Modes Specification](./addressing-modes)**  
  Detailed behavior, operand encodings, formula calculations, and timing penalties for all 13 modes.

- **[Arithmetic & Logical Opcodes](./opcodes-arithmetic-logic)**  
  Reference specifications for `ADC`, `SBC`, `AND`, `ORA`, `EOR`, `BIT`, `CMP`, `CPX`, `CPY`, `ASL`, `LSR`, `ROL`, `ROR`.

- **[Branch & Jump Opcodes](./opcodes-branch-jump)**  
  Reference specifications for `BCC`, `BCS`, `BEQ`, `BMI`, `BNE`, `BPL`, `BVC`, `BVS`, `JMP`, `JSR`, `RTS`, `RTI`.

- **[Memory & Register Transfer Opcodes](./opcodes-memory-transfers)**  
  Reference specifications for `LDA`, `LDX`, `LDY`, `STA`, `STX`, `STY`, `TAX`, `TAY`, `TSX`, `TXA`, `TXS`, `TYA`, `INC`, `INX`, `INY`, `DEC`, `DEX`, `DEY`.

- **[Stack & System Control Opcodes](./opcodes-stack-system)**  
  Reference specifications for `PHA`, `PHP`, `PLA`, `PLP`, `CLC`, `CLD`, `CLI`, `CLV`, `SEC`, `SED`, `SEI`, `BRK`, `NOP`.
