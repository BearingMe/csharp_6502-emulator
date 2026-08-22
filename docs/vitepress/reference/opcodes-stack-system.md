---
title: Stack & System Control Opcodes Reference
description: Complete specification for 6502 stack operations, flag manipulation, and processor control opcodes.
---

## Stack Operations

The 6502 stack is located in **Page 1 (`$0100-$01FF`)** and operates in a descending Last-In-First-Out (LIFO) manner.

| Mnemonic | Name | Opcode | Bytes | Cycles | Operation | Flags Affected |
| :--- | :--- | :---: | :---: | :---: | :--- | :---: |
| **PHA** | Push Accumulator | `$48` | 1 | 3 | $\text{Stack}[\text{SP}--] = A$ | None |
| **PHP** | Push Status Register | `$08` | 1 | 3 | $\text{Stack}[\text{SP}--] = \text{SR} \lor 0\text{x}30$ | None |
| **PLA** | Pull Accumulator | `$68` | 1 | 4 | $A = \text{Stack}[++\text{SP}]$ | `N`, `Z` |
| **PLP** | Pull Status Register | `$28` | 1 | 4 | $\text{SR} = \text{Stack}[++\text{SP}]$ | `N`, `V`, `D`, `I`, `Z`, `C` |

::: tip Break and Unused Bits on Stack
- `PHP` pushes the status register with **Bit 4 (B)** and **Bit 5 (U)** set to `1` (`0x30`).
- `PLP` restores bits 7, 6, 3, 2, 1, 0, but ignores bits 4 and 5.
:::

---

## Flag Clear & Set Instructions

All flag instructions use **Implied (`IMP`)** addressing and take **1 byte** and **2 cycles**.

| Mnemonic | Full Name | Opcode | Operation | Flag Effect |
| :--- | :--- | :---: | :--- | :--- |
| **CLC** | Clear Carry Flag | `$18` | $C = 0$ | $C \leftarrow 0$ |
| **SEC** | Set Carry Flag | `$38` | $C = 1$ | $C \leftarrow 1$ |
| **CLI** | Clear Interrupt Disable | `$58` | $I = 0$ (Enable IRQ) | $I \leftarrow 0$ |
| **SEI** | Set Interrupt Disable | `$78` | $I = 1$ (Disable IRQ) | $I \leftarrow 1$ |
| **CLV** | Clear Overflow Flag | `$B8` | $V = 0$ | $V \leftarrow 0$ |
| **CLD** | Clear Decimal Mode | `$D8` | $D = 0$ (Binary arithmetic) | $D \leftarrow 0$ |
| **SED** | Set Decimal Mode | `$F8` | $D = 1$ (BCD arithmetic) | $D \leftarrow 1$ |

---

## System Control & Miscellaneous

### BRK — Break (Software Interrupt)
- **Opcode**: `$00`
- **Bytes**: 1 (commonly followed by 1 padding byte)
- **Cycles**: 7
- **Operation**:
  1. Increment `PC` by 1.
  2. Push `PC_hi` to stack, decrement `SP`.
  3. Push `PC_lo` to stack, decrement `SP`.
  4. Push `Status` with $B=1$ and $U=1$ to stack, decrement `SP`.
  5. Set Interrupt Disable $I=1$.
  6. Set `PC = ($FFFE) | (($FFFF) << 8)`.

---

### NOP — No Operation
- **Opcode**: `$EA`
- **Bytes**: 1
- **Cycles**: 2
- **Operation**: Performs no state changes other than advancing `PC` and consuming 2 clock cycles.
- **Flags**: None.
