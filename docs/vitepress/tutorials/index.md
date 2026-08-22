---
title: Modular 6502 Emulator Tutorials
description: Step-by-step practical guides to building every subsystem of a MOS 6502 CPU emulator independently.
---

This tutorial series guides you through creating a modular, maintainable MOS 6502 CPU emulator from the ground up. Rather than treating the CPU as an opaque monolithic black box, each lesson focuses on designing and testing a single subsystem in isolation before connecting it to the larger system.

## Tutorial Path

1. **[Architecture & Memory Bus](./getting-started)**  
   Define the 16-bit address space, implement 8-bit read/write bus abstractions, and prepare RAM.

2. **[Register File & Status Flags](./registers-and-flags)**  
   Implement the 8-bit Accumulator, Index Registers (X, Y), Stack Pointer, 16-bit Program Counter, and Processor Status (P/SR) bit flags.

3. **[Addressing Mode Resolvers](./addressing-modes)**  
   Build modular address calculators for all 13 modes (Immediate, Zero Page, Absolute, Indexed, Relative, and Indirect).

4. **[Bitwise Math & Arithmetic (ALU)](./bitwise-math-and-alu)**  
   Construct the Arithmetic Logic Unit handling binary two's complement addition/subtraction, bitwise logic, shifts, and rotates.

5. **[Instruction Dispatcher & Stepper](./instruction-dispatcher)**  
   Assemble the 16x16 opcode lookup table, decode instruction cycles, and wire execution stepping.

6. **[Interrupts & System Vectors](./interrupts-and-reset)**  
   Implement hardware reset sequences, Maskable Interrupts (IRQ), Non-Maskable Interrupts (NMI), and the software `BRK` instruction.
