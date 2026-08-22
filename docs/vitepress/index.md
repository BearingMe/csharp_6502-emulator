---
layout: home

hero:
  name: "MOS 6502 Architecture & Emulation"
  text: "Modular Engineering Guide"
  tagline: "Build every component of the legendary 8-bit CPU: Bitwise Math & ALU, 13 Addressing Modes, Complete Instruction Matrix, and Memory Bus."
  actions:
    - theme: brand
      text: Get Started Building
      link: /tutorials/getting-started
    - theme: alt
      text: Instruction Reference
      link: /reference/instruction-matrix

features:
  - title: Modular CPU Construction
    details: Focused tutorials showing how to create each subsystem independently—registers, ALU, bus interconnect, address decoders, and cycle timer.
  - title: Bitwise Math & Arithmetic Logic
    details: Deep dive into binary two's complement, carry propagation, signed overflow determination, bitwise logic (AND/ORA/EOR/BIT), shifts, and rotates.
  - title: 13 Addressing Modes
    details: Precise resolution logic for Immediate, Zero Page, Absolute, Indexed (X/Y), Relative, Indirect, and Indexed Indirect (IZX/IZY) modes.
  - title: Full Instruction Matrix
    details: Complete 56-opcode official instruction set, cycle timings, byte lengths, addressing mappings, and flag side effects.
---

## Documentation Structure

The documentation follows the **Diátaxis** framework:

::: info Diátaxis Navigation
- **[Tutorials](/tutorials/)**: Step-by-step modular lessons taking you from basic memory bus connection to full CPU instruction execution.
- **[How-To Guides](/how-to/)**: Practical solutions for specific emulator tasks like calculating signed overflow, resolving IZX/IZY pointer indirection, and cycle counting.
- **[Technical Reference](/reference/)**: Exact specifications of all 56 official instructions, 13 addressing modes, status register bits, and hardware vectors.
- **[Explanation](/explanation/)**: In-depth theoretical discussions covering hardware architecture, two's complement arithmetic, and page boundary penalty mechanics.
:::
