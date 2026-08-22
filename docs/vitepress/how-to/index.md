---
title: Practical How-To Guides
description: Recipes and implementation patterns for 6502 math, addressing quirks, cycle counting, and disassembler construction.
---

The How-To section provides focused, task-oriented guides solving specific engineering challenges when building or verifying a MOS 6502 emulator.

## Available Guides

- **[Implement Two's Complement ADC / SBC](./implement-adc-sbc)**  
  Formula derivation, binary vs decimal arithmetic, signed overflow mask calculations, and carry propagation.

- **[Implement Bitwise Shifts & Rotates](./implement-shifts-and-rotates)**  
  Clean bit manipulation recipes for `ASL`, `LSR`, `ROL`, `ROR`, and `BIT`.

- **[Resolve Indexed Indirect Modes (IZX/IZY)](./resolve-indexed-indirect)**  
  Step-by-step memory pointer lookup logic for `($LL, X)` (Pre-indexed) and `($LL), Y` (Post-indexed).

- **[Emulate 6502 Page Boundary Penalties](./handle-page-boundary-cycles)**  
  Correctly detect 256-byte page crossings and add extra clock cycles for indexed loads and branch targets.

- **[Replicate the Indirect JMP Hardware Bug](./replicate-jmp-indirect-bug)**  
  Accurately reproduce the NMOS 6502 hardware bug when executing `JMP ($xxFF)`.

- **[Build a Step-by-Step CPU Disassembler](./build-6502-disassembler)**  
  Decode raw machine code in memory into formatted 6502 assembly with memory operands and addressing mode tags.
