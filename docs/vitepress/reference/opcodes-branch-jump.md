---
title: Branch & Jump Opcodes Reference
description: Complete reference for 6502 conditional branch opcodes, unconditional jumps, and subroutine calls.
---

## Conditional Branch Instructions

All conditional branches use **Relative (`REL`)** addressing (2 bytes).
- **Cycles**: 2 if branch is not taken, 3 if taken, 4 if taken across a 256-byte page boundary.
- **Flags Affected**: None.

| Mnemonic | Name | Opcode | Condition |
| :--- | :--- | :---: | :--- |
| **BCC** | Branch if Carry Clear | `$90` | $C = 0$ |
| **BCS** | Branch if Carry Set | `$B0` | $C = 1$ |
| **BEQ** | Branch if Equal | `$F0` | $Z = 1$ |
| **BNE** | Branch if Not Equal | `$D0` | $Z = 0$ |
| **BMI** | Branch if Minus | `$30` | $N = 1$ |
| **BPL** | Branch if Plus | `$10` | $N = 0$ |
| **BVC** | Branch if Overflow Clear | `$50` | $V = 0$ |
| **BVS** | Branch if Overflow Set | `$70` | $V = 1$ |

---

## Jumps & Subroutines

### JMP — Jump to Address
Sets Program Counter (`PC`) to target location. Flags: None.

| Mode | Opcode | Bytes | Cycles | Notes |
| :--- | :---: | :---: | :---: | :--- |
| Absolute | `$4C` | 3 | 3 | Direct jump to `$HHLL` |
| Indirect | `$6C` | 3 | 5 | Reads pointer from `$HHLL` (Page wrap bug at `$xxFF`) |

---

### JSR — Jump to Subroutine
Pushes address of the next instruction minus 1 (`PC + 2 - 1`) to the stack (high byte first, then low byte), then jumps to target address.
- **Flags**: None.

| Mode | Opcode | Bytes | Cycles | Notes |
| :--- | :---: | :---: | :---: | :--- |
| Absolute | `$20` | 3 | 6 | Pushes return address to stack |

---

### RTS — Return from Subroutine
Pulls return address (`low`, then `high`) from the stack, increments it by 1, and assigns to `PC`.
- **Flags**: None.

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Implied | `$60` | 1 | 6 |

---

### RTI — Return from Interrupt
Pulls Status Register (`P`) from stack (ignoring `B` and `U`), then pulls `PC` (`low`, then `high`).
- **Flags**: Restores all flags from stack.

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Implied | `$40` | 1 | 6 |
