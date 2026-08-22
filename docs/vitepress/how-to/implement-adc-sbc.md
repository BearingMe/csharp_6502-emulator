---
title: How to Implement Two's Complement ADC / SBC
description: Complete mathematical recipes for implementing Add with Carry (ADC) and Subtract with Borrow (SBC) in a 6502 emulator.
---

Implementing `ADC` (Add with Carry) and `SBC` (Subtract with Borrow) is one of the most error-prone parts of writing a 6502 emulator due to signed overflow detection and borrow logic.

## Goal

Correctly compute 8-bit arithmetic results while setting the **Carry (C)**, **Zero (Z)**, **Negative (N)**, and **Overflow (V)** flags according to hardware behavior.

---

## 1. Implementing `ADC` (Add with Carry)

### Formula
$$\text{temp} = A + M + C_{\text{in}}$$

### Flag Rules
- **Carry ($C$)**: Set if $\text{temp} > 255$ (unsigned overflow past bit 7).
- **Zero ($Z$)**: Set if $(\text{temp} \land 0\text{xFF}) == 0$.
- **Negative ($N$)**: Set if $(\text{temp} \land 0\text{x}80) \neq 0$ (MSB is 1).
- **Overflow ($V$)**: Set if adding two operands with the same sign produces a result with an opposite sign:
  $$V = \neg(A \oplus M) \land (A \oplus \text{temp}) \land 0\text{x}80$$

### Implementation Recipe

::: code-group
```cpp [C++]
uint8_t CPU6502::ADC() {
    fetch(); // Get operand M from memory or immediate byte

    uint16_t a_val = (uint16_t)a;
    uint16_t m_val = (uint16_t)fetched;
    uint16_t c_val = (uint16_t)getFlag(Flags6502::C);

    uint16_t temp = a_val + m_val + c_val;

    // Unsigned Carry out (bit 8)
    setFlag(Flags6502::C, temp > 255);

    // Zero flag
    setFlag(Flags6502::Z, (temp & 0x00FF) == 0);

    // Negative flag
    setFlag(Flags6502::N, (temp & 0x0080) != 0);

    // Signed Overflow flag (V)
    // Condition: (~(A ^ M) & (A ^ Result)) & 0x80
    setFlag(Flags6502::V, (~(a_val ^ m_val) & (a_val ^ temp)) & 0x0080);

    // Store lower 8 bits into Accumulator
    a = temp & 0x00FF;

    return 1; // Potential +1 cycle on page cross
}
```

```rust [Rust]
pub fn adc(&mut self) -> u8 {
    self.fetch(); // Get operand M from memory or immediate byte

    let a_val = self.a as u16;
    let m_val = self.fetched as u16;
    let c_val = if self.get_flag(Flag::C) { 1u16 } else { 0u16 };

    let temp = a_val + m_val + c_val;

    // Unsigned Carry out (bit 8)
    self.set_flag(Flag::C, temp > 255);

    // Zero flag
    self.set_flag(Flag::Z, (temp & 0x00FF) == 0);

    // Negative flag
    self.set_flag(Flag::N, (temp & 0x0080) != 0);

    // Signed Overflow flag (V)
    // Condition: (~(A ^ M) & (A ^ Result)) & 0x80
    let v = (!(a_val ^ m_val) & (a_val ^ temp)) & 0x0080;
    self.set_flag(Flag::V, v != 0);

    // Store lower 8 bits into Accumulator
    self.a = (temp & 0x00FF) as u8;

    1 // Potential +1 cycle on page cross
}
```

```typescript [TypeScript]
public ADC(): number {
  this.fetch(); // Get operand M from memory or immediate byte

  const aVal = this.a;
  const mVal = this.fetched;
  const cVal = this.getFlag(Flags6502.C);

  const temp = aVal + mVal + cVal;

  // Unsigned Carry out (bit 8)
  this.setFlag(Flags6502.C, temp > 255);

  // Zero flag
  this.setFlag(Flags6502.Z, (temp & 0x00FF) === 0);

  // Negative flag
  this.setFlag(Flags6502.N, (temp & 0x0080) !== 0);

  // Signed Overflow flag (V)
  // Condition: (~(A ^ M) & (A ^ Result)) & 0x80
  const v = (~(aVal ^ mVal) & (aVal ^ temp)) & 0x0080;
  this.setFlag(Flags6502.V, v !== 0);

  // Store lower 8 bits into Accumulator
  this.a = temp & 0x00FF;

  return 1; // Potential +1 cycle on page cross
}
```
:::

---

## 2. Implementing `SBC` (Subtract with Borrow)

### Mathematical Conversion
On the 6502, subtraction with borrow is defined as:
$$A - M - (1 - C_{\text{in}})$$

Using two's complement inversion:
$$-M = \sim M + 1$$
$$A - M - (1 - C) = A + (\sim M + 1) - 1 + C = A + (\sim M) + C$$

By simply inverting the bits of $M$ ($M \oplus 0\text{xFF}$), `SBC` runs the **exact same logic as `ADC`**!

### Implementation Recipe

::: code-group
```cpp [C++]
uint8_t CPU6502::SBC() {
    fetch();

    // Invert the operand bits
    uint16_t inverted_m = ((uint16_t)fetched) ^ 0x00FF;
    uint16_t a_val = (uint16_t)a;
    uint16_t c_val = (uint16_t)getFlag(Flags6502::C);

    uint16_t temp = a_val + inverted_m + c_val;

    // Carry out: bit 8 is set if NO borrow occurred (result >= 0)
    setFlag(Flags6502::C, (temp & 0xFF00) != 0);

    // Zero flag
    setFlag(Flags6502::Z, (temp & 0x00FF) == 0);

    // Negative flag
    setFlag(Flags6502::N, (temp & 0x0080) != 0);

    // Signed Overflow: Compare A, inverted M, and temp
    setFlag(Flags6502::V, ((temp ^ a_val) & (temp ^ inverted_m)) & 0x0080);

    a = temp & 0x00FF;

    return 1;
}
```

```rust [Rust]
pub fn sbc(&mut self) -> u8 {
    self.fetch();

    // Invert the operand bits
    let inverted_m = (self.fetched as u16) ^ 0x00FF;
    let a_val = self.a as u16;
    let c_val = if self.get_flag(Flag::C) { 1u16 } else { 0u16 };

    let temp = a_val + inverted_m + c_val;

    // Carry out: bit 8 is set if NO borrow occurred (result >= 0)
    self.set_flag(Flag::C, (temp & 0xFF00) != 0);

    // Zero flag
    self.set_flag(Flag::Z, (temp & 0x00FF) == 0);

    // Negative flag
    self.set_flag(Flag::N, (temp & 0x0080) != 0);

    // Signed Overflow: Compare A, inverted M, and temp
    let v = ((temp ^ a_val) & (temp ^ inverted_m)) & 0x0080;
    self.set_flag(Flag::V, v != 0);

    self.a = (temp & 0x00FF) as u8;

    1
}
```

```typescript [TypeScript]
public SBC(): number {
  this.fetch();

  // Invert the operand bits
  const invertedM = (this.fetched ^ 0x00FF) & 0xFFFF;
  const aVal = this.a;
  const cVal = this.getFlag(Flags6502.C);

  const temp = aVal + invertedM + cVal;

  // Carry out: bit 8 is set if NO borrow occurred (result >= 0)
  this.setFlag(Flags6502.C, (temp & 0xFF00) !== 0);

  // Zero flag
  this.setFlag(Flags6502.Z, (temp & 0x00FF) === 0);

  // Negative flag
  this.setFlag(Flags6502.N, (temp & 0x0080) !== 0);

  // Signed Overflow: Compare A, inverted M, and temp
  const v = ((temp ^ aVal) & (temp ^ invertedM)) & 0x0080;
  this.setFlag(Flags6502.V, v !== 0);

  this.a = temp & 0x00FF;

  return 1;
}
```
:::

---

## Test Cases for Verification

| Operation | Inputs | Expected Result | Flags |
| :--- | :--- | :--- | :--- |
| `ADC` Positive + Positive | $A = 0\text{x}40$ (+64), $M = 0\text{x}20$ (+32), $C = 0$ | $A = 0\text{x}60$ (+96) | $C=0, Z=0, N=0, V=0$ |
| `ADC` Positive Overflow | $A = 0\text{x}50$ (+80), $M = 0\text{x}50$ (+80), $C = 0$ | $A = 0\text{x}A0$ (-96) | $C=0, Z=0, N=1, V=1$ |
| `ADC` Unsigned Carry Out | $A = 0\text{x}FF$ (255), $M = 0\text{x}01$ (1), $C = 0$ | $A = 0\text{x}00$ (0) | $C=1, Z=1, N=0, V=0$ |
| `SBC` No Borrow Needed | $A = 0\text{x}10$ (16), $M = 0\text{x}05$ (5), $C = 1$ | $A = 0\text{x}0B$ (11) | $C=1, Z=0, N=0, V=0$ |
| `SBC` Borrow Occurred | $A = 0\text{x}05$ (5), $M = 0\text{x}0A$ (10), $C = 1$ | $A = 0\text{x}FB$ (-5) | $C=0, Z=0, N=1, V=0$ |
| `SBC` Negative Overflow | $A = 0\text{x}80$ (-128), $M = 0\text{x}01$ (1), $C = 1$ | $A = 0\text{x}7F$ (+127) | $C=1, Z=0, N=0, V=1$ |
