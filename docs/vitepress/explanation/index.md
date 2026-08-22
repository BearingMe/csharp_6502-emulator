---
title: Explanation & Architecture Overview
description: Conceptual foundations, hardware architecture, two's complement arithmetic, and execution models of the MOS 6502.
---

The Explanation quadrant provides the conceptual understanding, mathematical foundations, and hardware rationale behind the MOS 6502 microprocessor.

## Core Topics

- **[6502 Hardware Architecture](./hardware-architecture)**  
  Internal block diagram, register bus topology, ALU structure, timing phases ($\Phi_1, \Phi_2$), and control logic.

- **[Overflow & Signed Binary Arithmetic](./signed-overflow-mechanics)**  
  Mathematical proof and boolean logic derivations for signed two's complement overflow in `ADC` and `SBC`.

- **[Cycle-Accurate vs Step Execution](./cycle-execution-model)**  
  Comparison between sub-cycle state machine emulation and instruction-level cycle countdown timers.

- **[Memory Map & Page Structure](./memory-organization-and-stack)**  
  Zero Page performance rationale, Page 1 descending stack mechanics, memory-mapped I/O, and hardware interrupt vectors.
