---
title: Arithmetic & Logical Opcodes Reference
description: Complete specification for 6502 ADC, SBC, AND, ORA, EOR, BIT, CMP, CPX, CPY, ASL, LSR, ROL, and ROR opcodes.
---

## Arithmetic Instructions

### ADC — Add with Carry
- **Operation**: $A = A + M + C$
- **Flags**: `N`, `Z`, `C`, `V`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Immediate | `$69` | 2 | 2 |
| Zero Page | `$65` | 2 | 3 |
| Zero Page, X | `$75` | 2 | 4 |
| Absolute | `$6D` | 3 | 4 |
| Absolute, X | `$7D` | 3 | 4* |
| Absolute, Y | `$79` | 3 | 4* |
| (Indirect, X) | `$61` | 2 | 6 |
| (Indirect), Y | `$71` | 2 | 5* |

*\* +1 cycle if page boundary is crossed*

---

### SBC — Subtract with Borrow
- **Operation**: $A = A - M - (1 - C)$
- **Flags**: `N`, `Z`, `C`, `V`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Immediate | `$E9` | 2 | 2 |
| Zero Page | `$E5` | 2 | 3 |
| Zero Page, X | `$F5` | 2 | 4 |
| Absolute | `$ED` | 3 | 4 |
| Absolute, X | `$FD` | 3 | 4* |
| Absolute, Y | `$F9` | 3 | 4* |
| (Indirect, X) | `$E1` | 2 | 6 |
| (Indirect), Y | `$F1` | 2 | 5* |

---

## Logical & Bit Test Instructions

### AND — Bitwise AND with Accumulator
- **Operation**: $A = A \land M$
- **Flags**: `N`, `Z`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Immediate | `$29` | 2 | 2 |
| Zero Page | `$25` | 2 | 3 |
| Zero Page, X | `$35` | 2 | 4 |
| Absolute | `$2D` | 3 | 4 |
| Absolute, X | `$3D` | 3 | 4* |
| Absolute, Y | `$39` | 3 | 4* |
| (Indirect, X) | `$21` | 2 | 6 |
| (Indirect), Y | `$31` | 2 | 5* |

---

### ORA — Bitwise OR with Accumulator
- **Operation**: $A = A \lor M$
- **Flags**: `N`, `Z`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Immediate | `$09` | 2 | 2 |
| Zero Page | `$05` | 2 | 3 |
| Zero Page, X | `$15` | 2 | 4 |
| Absolute | `$0D` | 3 | 4 |
| Absolute, X | `$1D` | 3 | 4* |
| Absolute, Y | `$19` | 3 | 4* |
| (Indirect, X) | `$01` | 2 | 6 |
| (Indirect), Y | `$11` | 2 | 5* |

---

### EOR — Bitwise Exclusive OR with Accumulator
- **Operation**: $A = A \oplus M$
- **Flags**: `N`, `Z`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Immediate | `$49` | 2 | 2 |
| Zero Page | `$45` | 2 | 3 |
| Zero Page, X | `$55` | 2 | 4 |
| Absolute | `$4D` | 3 | 4 |
| Absolute, X | `$5D` | 3 | 4* |
| Absolute, Y | `$59` | 3 | 4* |
| (Indirect, X) | `$41` | 2 | 6 |
| (Indirect), Y | `$51` | 2 | 5* |

---

### BIT — Bit Test
- **Operation**: $A \land M \rightarrow Z,\quad M_7 \rightarrow N,\quad M_6 \rightarrow V$
- **Flags**: `N`, `Z`, `V`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Zero Page | `$24` | 2 | 3 |
| Absolute | `$2C` | 3 | 4 |

---

## Comparison Instructions

### CMP — Compare Accumulator
- **Operation**: $A - M \rightarrow \text{Flags}$
- **Flags**: `N`, `Z`, `C` (Carry set if $A \ge M$)

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Immediate | `$C9` | 2 | 2 |
| Zero Page | `$C5` | 2 | 3 |
| Zero Page, X | `$D5` | 2 | 4 |
| Absolute | `$CD` | 3 | 4 |
| Absolute, X | `$DD` | 3 | 4* |
| Absolute, Y | `$D9` | 3 | 4* |
| (Indirect, X) | `$C1` | 2 | 6 |
| (Indirect), Y | `$D1` | 2 | 5* |

---

### CPX — Compare X Register
- **Operation**: $X - M \rightarrow \text{Flags}$
- **Flags**: `N`, `Z`, `C` (Carry set if $X \ge M$)

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Immediate | `$E0` | 2 | 2 |
| Zero Page | `$E4` | 2 | 3 |
| Absolute | `$EC` | 3 | 4 |

---

### CPY — Compare Y Register
- **Operation**: $Y - M \rightarrow \text{Flags}$
- **Flags**: `N`, `Z`, `C` (Carry set if $Y \ge M$)

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Immediate | `$C0` | 2 | 2 |
| Zero Page | `$C4` | 2 | 3 |
| Absolute | `$CC` | 3 | 4 |

---

## Shifts & Rotates

### ASL — Arithmetic Shift Left
- **Operation**: $C \leftarrow [7..0] \leftarrow 0$
- **Flags**: `N`, `Z`, `C`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Accumulator | `$0A` | 1 | 2 |
| Zero Page | `$06` | 2 | 5 |
| Zero Page, X | `$16` | 2 | 6 |
| Absolute | `$0E` | 3 | 6 |
| Absolute, X | `$1E` | 3 | 7 |

---

### LSR — Logical Shift Right
- **Operation**: $0 \rightarrow [7..0] \rightarrow C$
- **Flags**: `N` (cleared), `Z`, `C`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Accumulator | `$4A` | 1 | 2 |
| Zero Page | `$46` | 2 | 5 |
| Zero Page, X | `$56` | 2 | 6 |
| Absolute | `$4E` | 3 | 6 |
| Absolute, X | `$5E` | 3 | 7 |

---

### ROL — Rotate Left through Carry
- **Operation**: $C \leftarrow [7..0] \leftarrow C$
- **Flags**: `N`, `Z`, `C`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Accumulator | `$2A` | 1 | 2 |
| Zero Page | `$26` | 2 | 5 |
| Zero Page, X | `$36` | 2 | 6 |
| Absolute | `$2E` | 3 | 6 |
| Absolute, X | `$3E` | 3 | 7 |

---

### ROR — Rotate Right through Carry
- **Operation**: $C \rightarrow [7..0] \rightarrow C$
- **Flags**: `N`, `Z`, `C`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Accumulator | `$6A` | 1 | 2 |
| Zero Page | `$66` | 2 | 5 |
| Zero Page, X | `$76` | 2 | 6 |
| Absolute | `$6E` | 3 | 6 |
| Absolute, X | `$7E` | 3 | 7 |
