---
title: Signed Overflow Mechanics & Two's Complement Math
description: Mathematical derivation, truth tables, and boolean logic for signed overflow detection in 6502 ADC and SBC.
---

A frequent challenge in 6502 emulation is correctly calculating the **Overflow ($V$)** flag during addition (`ADC`) and subtraction (`SBC`).

## Signed vs. Unsigned Interpretation

An 8-bit byte represents integers in two distinct ways:

- **Unsigned Range**: $0$ to $255$ (`0x00` to `0xFF`).
- **Signed Two's Complement Range**: $-128$ to $+127$.
  - `0x00` ($0$) to `0x7F` ($+127$) are positive (Bit 7 = `0`).
  - `0x80` ($-128$) to `0xFF` ($-1$) are negative (Bit 7 = `1`).

The hardware adder adds the binary bits identically regardless of signed or unsigned context. The processor sets:
- **Carry Flag ($C$)** to indicate **unsigned overflow** past 255.
- **Overflow Flag ($V$)** to indicate **signed overflow** outside $[-128, +127]$.

---

## When Does Signed Overflow Occur?

Signed overflow occurs if and only if:
1. Two **positive** numbers are added and yield a **negative** result:
   $$\text{Positive} + \text{Positive} = \text{Negative} \implies \text{Overflow } (V=1)$$
2. Two **negative** numbers are added and yield a **positive** result:
   $$\text{Negative} + \text{Negative} = \text{Positive} \implies \text{Overflow } (V=1)$$
3. Adding a positive number and a negative number can **never** overflow because the magnitude of the sum is always smaller than the larger operand:
   $$\text{Positive} + \text{Negative} = \text{Always in range } \implies V=0$$

---

## Truth Table & Boolean Derivation

Let:
- $A_7$ = Bit 7 (sign bit) of Accumulator
- $M_7$ = Bit 7 (sign bit) of Memory Operand
- $R_7$ = Bit 7 (sign bit) of Result ($\text{Sum} = A + M + C$)

| $A_7$ | $M_7$ | $R_7$ | Description | Overflow ($V$) | $A_7 \oplus R_7$ | $\neg(A_7 \oplus M_7)$ |
| :---: | :---: | :---: | :--- | :---: | :---: | :---: |
| 0 | 0 | 0 | Pos + Pos = Pos | **0** | 0 | 1 |
| 0 | 0 | 1 | Pos + Pos = Neg | **1** | 1 | 1 |
| 0 | 1 | 0 | Pos + Neg = Pos | **0** | 0 | 0 |
| 0 | 1 | 1 | Pos + Neg = Neg | **0** | 1 | 0 |
| 1 | 0 | 0 | Neg + Pos = Pos | **0** | 1 | 0 |
| 1 | 0 | 1 | Neg + Pos = Neg | **0** | 0 | 0 |
| 1 | 1 | 0 | Neg + Neg = Pos | **1** | 1 | 1 |
| 1 | 1 | 1 | Neg + Neg = Neg | **0** | 0 | 1 |

From this table, $V$ is true only when:
- $A_7$ and $M_7$ match ($\neg(A_7 \oplus M_7) = 1$), AND
- $A_7$ and $R_7$ differ ($A_7 \oplus R_7 = 1$).

Thus, the exact boolean formula for `ADC` overflow is:

$$V = \neg(A \oplus M) \land (A \oplus R) \land 0\text{x}80$$

---

## Subtraction (`SBC`) Inversion

In two's complement arithmetic, subtraction $A - M - (1 - C)$ is converted to addition:

$$A + (\sim M) + C$$

By defining $M_{\text{inverted}} = M \oplus 0\text{xFF}$, the exact same equation applies:

$$V = (R \oplus A) \land (R \oplus M_{\text{inverted}}) \land 0\text{x}80$$
