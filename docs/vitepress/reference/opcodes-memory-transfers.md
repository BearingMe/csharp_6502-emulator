---
title: Memory & Register Transfer Opcodes Reference
description: Reference specifications for load, store, transfer, increment, and decrement instructions.
---

## Load & Store Instructions

### LDA — Load Accumulator
- **Operation**: $A = M$
- **Flags**: `N`, `Z`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Immediate | `$A9` | 2 | 2 |
| Zero Page | `$A5` | 2 | 3 |
| Zero Page, X | `$B5` | 2 | 4 |
| Absolute | `$AD` | 3 | 4 |
| Absolute, X | `$BD` | 3 | 4* |
| Absolute, Y | `$B9` | 3 | 4* |
| (Indirect, X) | `$A1` | 2 | 6 |
| (Indirect), Y | `$B1` | 2 | 5* |

*\* +1 cycle if page boundary crossed*

---

### LDX — Load X Register
- **Operation**: $X = M$
- **Flags**: `N`, `Z`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Immediate | `$A2` | 2 | 2 |
| Zero Page | `$A6` | 2 | 3 |
| Zero Page, Y | `$B6` | 2 | 4 |
| Absolute | `$AE` | 3 | 4 |
| Absolute, Y | `$BE` | 3 | 4* |

---

### LDY — Load Y Register
- **Operation**: $Y = M$
- **Flags**: `N`, `Z`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Immediate | `$A0` | 2 | 2 |
| Zero Page | `$A4` | 2 | 3 |
| Zero Page, X | `$B4` | 2 | 4 |
| Absolute | `$AC` | 3 | 4 |
| Absolute, X | `$BC` | 3 | 4* |

---

### STA — Store Accumulator
- **Operation**: $M = A$
- **Flags**: None

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Zero Page | `$85` | 2 | 3 |
| Zero Page, X | `$95` | 2 | 4 |
| Absolute | `$8D` | 3 | 4 |
| Absolute, X | `$9D` | 3 | 5 |
| Absolute, Y | `$99` | 3 | 5 |
| (Indirect, X) | `$81` | 2 | 6 |
| (Indirect), Y | `$91` | 2 | 6 |

---

### STX — Store X Register
- **Operation**: $M = X$
- **Flags**: None

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Zero Page | `$86` | 2 | 3 |
| Zero Page, Y | `$96` | 2 | 4 |
| Absolute | `$8E` | 3 | 4 |

---

### STY — Store Y Register
- **Operation**: $M = Y$
- **Flags**: None

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Zero Page | `$84` | 2 | 3 |
| Zero Page, X | `$94` | 2 | 4 |
| Absolute | `$8C` | 3 | 4 |

---

## Register Transfer Instructions

| Mnemonic | Opcode | Bytes | Cycles | Operation | Flags Affected |
| :--- | :---: | :---: | :---: | :--- | :---: |
| **TAX** | `$AA` | 1 | 2 | $X = A$ | `N`, `Z` |
| **TAY** | `$A8` | 1 | 2 | $Y = A$ | `N`, `Z` |
| **TSX** | `$BA` | 1 | 2 | $X = \text{SP}$ | `N`, `Z` |
| **TXA** | `$8A` | 1 | 2 | $A = X$ | `N`, `Z` |
| **TXS** | `$9A` | 1 | 2 | $\text{SP} = X$ | None |
| **TYA** | `$98` | 1 | 2 | $A = Y$ | `N`, `Z` |

---

## Increments & Decrements

### INC — Increment Memory
- **Operation**: $M = M + 1$
- **Flags**: `N`, `Z`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Zero Page | `$E6` | 2 | 5 |
| Zero Page, X | `$F6` | 2 | 6 |
| Absolute | `$EE` | 3 | 6 |
| Absolute, X | `$FE` | 3 | 7 |

---

### DEC — Decrement Memory
- **Operation**: $M = M - 1$
- **Flags**: `N`, `Z`

| Mode | Opcode | Bytes | Cycles |
| :--- | :---: | :---: | :---: |
| Zero Page | `$C6` | 2 | 5 |
| Zero Page, X | `$D6` | 2 | 6 |
| Absolute | `$CE` | 3 | 6 |
| Absolute, X | `$DE` | 3 | 7 |

---

### INX, INY, DEX, DEY
| Mnemonic | Opcode | Bytes | Cycles | Operation | Flags |
| :--- | :---: | :---: | :---: | :--- | :---: |
| **INX** | `$E8` | 1 | 2 | $X = X + 1$ | `N`, `Z` |
| **INY** | `$C8` | 1 | 2 | $Y = Y + 1$ | `N`, `Z` |
| **DEX** | `$CA` | 1 | 2 | $X = X - 1$ | `N`, `Z` |
| **DEY** | `$88` | 1 | 2 | $Y = Y - 1$ | `N`, `Z` |
