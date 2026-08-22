---
title: 6502 Hardware Architecture & Internal Organization
description: Analysis of the internal bus layout, ALU, register control, and two-phase clocking of the MOS 6502 CPU.
---

Designed by Chuck Peddle, Bill Mensch, and the MOS Technology team in 1975, the MOS 6502 revolutionized microcomputing by reducing chip complexity and die size while preserving high processing throughput.

## Internal Block Architecture

The 6502 architecture consists of three main internal components interconnected by dual internal data buses (**SB** / **DB**):

```text
                  ┌──────────────────────────────┐
                  │      Instruction Register    │
                  │              & PLA           │
                  └──────────────┬───────────────┘
                                 │ Control Signals
 ┌───────────────────────────────▼─────────────────────────────────┐
 │                       Internal Data Bus                         │
 └───┬──────────┬──────────┬───────────┬──────────┬──────────┬─────┘
     │          │          │           │          │          │
 ┌───▼───┐  ┌───▼───┐  ┌───▼───┐   ┌───▼───┐  ┌───▼───┐  ┌───▼───┐
 │ Accum │  │   X   │  │   Y   │   │   S   │  │ PCL / │  │  ALU  │
 │ (A)   │  │  (X)  │  │  (Y)  │   │  (SP) │  │  PCH  │  │       │
 └───┬───┘  └───┬───┘  └───┬───┘   └───┬───┘  └───┬───┘  └───┬───┘
     └──────────┴──────────┴───────────┴──────────┴──────────┘
                                 │
                   ┌─────────────▼─────────────┐
                   │  Address Bus Buffer (A0)  │
                   └─────────────┬─────────────┘
                                 │ (16-bit Address Bus)
```

### 1. Control Logic & PLA
Instead of a complex microcode ROM, the 6502 uses a hardwired **Programmable Logic Array (PLA)**. The PLA decodes the 8-bit opcode and current cycle step directly into discrete control lines, activating registers, bus gates, and ALU functions with minimal propagation delay.

### 2. Dual Internal Buses (SB and DB)
The processor contains two 8-bit internal buses:
- **DB (Data Bus)**: Connects external data pins to registers, ALU input, and instruction latch.
- **SB (Special / System Bus)**: Connects index registers, stack pointer, and address generation logic.

This dual-bus design allows the CPU to perform address arithmetic (such as adding an index register) concurrently with data fetching.

### 3. Arithmetic Logic Unit (ALU)
The 8-bit ALU performs addition, subtraction, bitwise logic (`AND`, `ORA`, `EOR`), and bit shifting. It has two input latches:
- **Input A**: Typically fed from the Accumulator.
- **Input B**: Fed from the internal data bus (memory operand or immediate byte).

---

## Two-Phase Clock System ($\Phi_1$ and $\Phi_2$)

The 6502 uses a non-overlapping two-phase clock:
- **Phase 1 ($\Phi_1$)**: The CPU computes addresses, precharges internal buses, and outputs the address onto the 16-bit address bus pins.
- **Phase 2 ($\Phi_2$)**: Memory and external devices respond by placing data onto or latching data from the 8-bit data bus. Internal register latches capture the results.

This clean half-cycle division allows systems like the Apple II and Commodore 64 to interleave video memory access during $\Phi_1$ and CPU memory access during $\Phi_2$ without bus contention or wait states.
