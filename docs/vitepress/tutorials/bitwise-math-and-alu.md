---
title: 4. Bitwise Math & Arithmetic (ALU)
description: Building the 6502 Arithmetic Logic Unit covering bitwise logic, shifts, rotates, and two's complement ADC/SBC.
---

The Arithmetic Logic Unit (ALU) is the core computational engine of the 6502. It operates on the Accumulator (`A`) and an 8-bit memory/operand value (`fetched`), setting flags according to the outcome.

## The Bitwise Operations

The bitwise instructions are straightforward and modify the **Negative (N)** and **Zero (Z)** flags based on the result.

### 1. AND, ORA, EOR

::: code-group
```cpp [C++]
// AND: Bitwise AND with Accumulator (A = A & M)
uint8_t CPU6502::AND() {
    fetch();
    a = a & fetched;
    setFlag(Z, a == 0x00);
    setFlag(N, (a & 0x80) != 0);
    return 1; // Can require extra cycle with page crossing
}

// ORA: Bitwise OR with Accumulator (A = A | M)
uint8_t CPU6502::ORA() {
    fetch();
    a = a | fetched;
    setFlag(Z, a == 0x00);
    setFlag(N, (a & 0x80) != 0);
    return 1;
}

// EOR: Bitwise Exclusive OR with Accumulator (A = A ^ M)
uint8_t CPU6502::EOR() {
    fetch();
    a = a ^ fetched;
    setFlag(Z, a == 0x00);
    setFlag(N, (a & 0x80) != 0);
    return 1;
}
```

```rust [Rust]
// AND: Bitwise AND with Accumulator (A = A & M)
pub fn and(&mut self) -> u8 {
    self.fetch();
    self.a &= self.fetched;
    self.update_nz(self.a);
    1 // Can require extra cycle with page crossing
}

// ORA: Bitwise OR with Accumulator (A = A | M)
pub fn ora(&mut self) -> u8 {
    self.fetch();
    self.a |= self.fetched;
    self.update_nz(self.a);
    1
}

// EOR: Bitwise Exclusive OR with Accumulator (A = A ^ M)
pub fn eor(&mut self) -> u8 {
    self.fetch();
    self.a ^= self.fetched;
    self.update_nz(self.a);
    1
}
```

```typescript [TypeScript]
// AND: Bitwise AND with Accumulator (A = A & M)
public AND(): number {
  this.fetch();
  this.a = (this.a & this.fetched) & 0xFF;
  this.updateNZ(this.a);
  return 1; // Can require extra cycle with page crossing
}

// ORA: Bitwise OR with Accumulator (A = A | M)
public ORA(): number {
  this.fetch();
  this.a = (this.a | this.fetched) & 0xFF;
  this.updateNZ(this.a);
  return 1;
}

// EOR: Bitwise Exclusive OR with Accumulator (A = A ^ M)
public EOR(): number {
  this.fetch();
  this.a = (this.a ^ this.fetched) & 0xFF;
  this.updateNZ(this.a);
  return 1;
}
```
:::

### 2. BIT (Bit Test)
`BIT` tests memory bits against the accumulator without modifying `A`.
- `Z` flag: set if `(A & M) == 0`.
- `N` flag: set directly from bit 7 of operand `M` (`M & 0x80`).
- `V` flag: set directly from bit 6 of operand `M` (`M & 0x40`).

::: code-group
```cpp [C++]
uint8_t CPU6502::BIT() {
    fetch();
    uint8_t temp = a & fetched;
    setFlag(Z, (temp & 0xFF) == 0x00);
    setFlag(N, (fetched & (1 << 7)) != 0);
    setFlag(V, (fetched & (1 << 6)) != 0);
    return 0;
}
```

```rust [Rust]
pub fn bit(&mut self) -> u8 {
    self.fetch();
    let temp = self.a & self.fetched;
    self.set_flag(Flag::Z, temp == 0x00);
    self.set_flag(Flag::N, (self.fetched & (1 << 7)) != 0);
    self.set_flag(Flag::V, (self.fetched & (1 << 6)) != 0);
    0
}
```

```typescript [TypeScript]
public BIT(): number {
  this.fetch();
  const temp = (this.a & this.fetched) & 0xFF;
  this.setFlag(Flags6502.Z, temp === 0x00);
  this.setFlag(Flags6502.N, (this.fetched & (1 << 7)) !== 0);
  this.setFlag(Flags6502.V, (this.fetched & (1 << 6)) !== 0);
  return 0;
}
```
:::

## Shifts and Rotates

All shifts and rotates capture the shifted-out bit into the **Carry (C)** flag.

```text
ASL (Shift Left):    C <─ [7 6 5 4 3 2 1 0] <─ 0
LSR (Shift Right):   0 ─> [7 6 5 4 3 2 1 0] ─> C
ROL (Rotate Left):   C <─ [7 6 5 4 3 2 1 0] <─ C
ROR (Rotate Right):  C ─> [7 6 5 4 3 2 1 0] ─> C
```

::: code-group
```cpp [C++]
// ASL: Arithmetic Shift Left
uint8_t CPU6502::ASL() {
    fetch();
    uint16_t temp = (uint16_t)fetched << 1;
    setFlag(C, (temp & 0xFF00) > 0);
    setFlag(Z, (temp & 0x00FF) == 0x00);
    setFlag(N, (temp & 0x80) != 0);
    if (lookup[opcode].addrmode == &CPU6502::IMP)
        a = temp & 0x00FF;
    else
        write(addr_abs, temp & 0x00FF);
    return 0;
}

// LSR: Logical Shift Right
uint8_t CPU6502::LSR() {
    fetch();
    setFlag(C, (fetched & 0x01) != 0);
    uint8_t temp = fetched >> 1;
    setFlag(Z, temp == 0x00);
    setFlag(N, false); // Bit 7 is always 0
    if (lookup[opcode].addrmode == &CPU6502::IMP)
        a = temp;
    else
        write(addr_abs, temp);
    return 0;
}

// ROL: Rotate Left through Carry
uint8_t CPU6502::ROL() {
    fetch();
    uint16_t temp = ((uint16_t)fetched << 1) | getFlag(C);
    setFlag(C, (temp & 0xFF00) > 0);
    setFlag(Z, (temp & 0x00FF) == 0x00);
    setFlag(N, (temp & 0x80) != 0);
    if (lookup[opcode].addrmode == &CPU6502::IMP)
        a = temp & 0x00FF;
    else
        write(addr_abs, temp & 0x00FF);
    return 0;
}

// ROR: Rotate Right through Carry
uint8_t CPU6502::ROR() {
    fetch();
    uint8_t oldCarry = getFlag(C);
    setFlag(C, (fetched & 0x01) != 0);
    uint8_t temp = (oldCarry << 7) | (fetched >> 1);
    setFlag(Z, temp == 0x00);
    setFlag(N, (temp & 0x80) != 0);
    if (lookup[opcode].addrmode == &CPU6502::IMP)
        a = temp;
    else
        write(addr_abs, temp);
    return 0;
}
```

```rust [Rust]
// ASL: Arithmetic Shift Left
pub fn asl(&mut self) -> u8 {
    self.fetch();
    let temp = (self.fetched as u16) << 1;
    self.set_flag(Flag::C, (temp & 0xFF00) > 0);
    self.set_flag(Flag::Z, (temp & 0x00FF) == 0x00);
    self.set_flag(Flag::N, (temp & 0x80) != 0);
    if self.lookup[self.opcode as usize].addrmode == AddrMode::IMP {
        self.a = (temp & 0x00FF) as u8;
    } else {
        self.write(self.addr_abs, (temp & 0x00FF) as u8);
    }
    0
}

// LSR: Logical Shift Right
pub fn lsr(&mut self) -> u8 {
    self.fetch();
    self.set_flag(Flag::C, (self.fetched & 0x01) != 0);
    let temp = self.fetched >> 1;
    self.set_flag(Flag::Z, temp == 0x00);
    self.set_flag(Flag::N, false);
    if self.lookup[self.opcode as usize].addrmode == AddrMode::IMP {
        self.a = temp;
    } else {
        self.write(self.addr_abs, temp);
    }
    0
}

// ROL: Rotate Left through Carry
pub fn rol(&mut self) -> u8 {
    self.fetch();
    let temp = ((self.fetched as u16) << 1) | (if self.get_flag(Flag::C) { 1 } else { 0 });
    self.set_flag(Flag::C, (temp & 0xFF00) > 0);
    self.set_flag(Flag::Z, (temp & 0x00FF) == 0x00);
    self.set_flag(Flag::N, (temp & 0x80) != 0);
    if self.lookup[self.opcode as usize].addrmode == AddrMode::IMP {
        self.a = (temp & 0x00FF) as u8;
    } else {
        self.write(self.addr_abs, (temp & 0x00FF) as u8);
    }
    0
}

// ROR: Rotate Right through Carry
pub fn ror(&mut self) -> u8 {
    self.fetch();
    let old_carry = if self.get_flag(Flag::C) { 1 } else { 0 };
    self.set_flag(Flag::C, (self.fetched & 0x01) != 0);
    let temp = (old_carry << 7) | (self.fetched >> 1);
    self.set_flag(Flag::Z, temp == 0x00);
    self.set_flag(Flag::N, (temp & 0x80) != 0);
    if self.lookup[self.opcode as usize].addrmode == AddrMode::IMP {
        self.a = temp;
    } else {
        self.write(self.addr_abs, temp);
    }
    0
}
```

```typescript [TypeScript]
// ASL: Arithmetic Shift Left
public ASL(): number {
  this.fetch();
  const temp = (this.fetched << 1) & 0xFFFF;
  this.setFlag(Flags6502.C, (temp & 0xFF00) > 0);
  this.setFlag(Flags6502.Z, (temp & 0x00FF) === 0x00);
  this.setFlag(Flags6502.N, (temp & 0x80) !== 0);
  if (this.lookup[this.opcode].addrmode === this.IMP) {
    this.a = temp & 0x00FF;
  } else {
    this.write(this.addrAbs, temp & 0x00FF);
  }
  return 0;
}

// LSR: Logical Shift Right
public LSR(): number {
  this.fetch();
  this.setFlag(Flags6502.C, (this.fetched & 0x01) !== 0);
  const temp = (this.fetched >> 1) & 0xFF;
  this.setFlag(Flags6502.Z, temp === 0x00);
  this.setFlag(Flags6502.N, false);
  if (this.lookup[this.opcode].addrmode === this.IMP) {
    this.a = temp;
  } else {
    this.write(this.addrAbs, temp);
  }
  return 0;
}

// ROL: Rotate Left through Carry
public ROL(): number {
  this.fetch();
  const temp = ((this.fetched << 1) | this.getFlag(Flags6502.C)) & 0xFFFF;
  this.setFlag(Flags6502.C, (temp & 0xFF00) > 0);
  this.setFlag(Flags6502.Z, (temp & 0x00FF) === 0x00);
  this.setFlag(Flags6502.N, (temp & 0x80) !== 0);
  if (this.lookup[this.opcode].addrmode === this.IMP) {
    this.a = temp & 0x00FF;
  } else {
    this.write(this.addrAbs, temp & 0x00FF);
  }
  return 0;
}

// ROR: Rotate Right through Carry
public ROR(): number {
  this.fetch();
  const oldCarry = this.getFlag(Flags6502.C);
  this.setFlag(Flags6502.C, (this.fetched & 0x01) !== 0);
  const temp = ((oldCarry << 7) | (this.fetched >> 1)) & 0xFF;
  this.setFlag(Flags6502.Z, temp === 0x00);
  this.setFlag(Flags6502.N, (temp & 0x80) !== 0);
  if (this.lookup[this.opcode].addrmode === this.IMP) {
    this.a = temp;
  } else {
    this.write(this.addrAbs, temp);
  }
  return 0;
}
```
:::

## Arithmetic: ADC & SBC

### ADC: Add with Carry
Equation: $A = A + M + C$

The signed overflow flag $V$ is set when adding two numbers of the same sign produces a result of the opposite sign:

$$\text{Overflow } V = \neg (A \oplus M) \land (A \oplus \text{Result}) \land 0\text{x}80$$

::: code-group
```cpp [C++]
uint8_t CPU6502::ADC() {
    fetch();
    uint16_t temp = (uint16_t)a + (uint16_t)fetched + (uint16_t)getFlag(C);

    // Carry flag: set if result exceeds 255 (unsigned overflow)
    setFlag(C, temp > 255);
    setFlag(Z, (temp & 0x00FF) == 0);
    setFlag(N, (temp & 0x80) != 0);

    // Signed Overflow: V = ~(A ^ M) & (A ^ temp) & 0x80
    setFlag(V, (~((uint16_t)a ^ (uint16_t)fetched) & ((uint16_t)a ^ temp)) & 0x0080);

    a = temp & 0x00FF;
    return 1;
}
```

```rust [Rust]
pub fn adc(&mut self) -> u8 {
    self.fetch();
    let carry = if self.get_flag(Flag::C) { 1u16 } else { 0u16 };
    let temp = (self.a as u16) + (self.fetched as u16) + carry;

    self.set_flag(Flag::C, temp > 255);
    self.set_flag(Flag::Z, (temp & 0x00FF) == 0);
    self.set_flag(Flag::N, (temp & 0x80) != 0);

    let v = (!((self.a as u16) ^ (self.fetched as u16)) & ((self.a as u16) ^ temp)) & 0x0080;
    self.set_flag(Flag::V, v != 0);

    self.a = (temp & 0x00FF) as u8;
    1
}
```

```typescript [TypeScript]
public ADC(): number {
  this.fetch();
  const temp = this.a + this.fetched + this.getFlag(Flags6502.C);

  this.setFlag(Flags6502.C, temp > 255);
  this.setFlag(Flags6502.Z, (temp & 0x00FF) === 0);
  this.setFlag(Flags6502.N, (temp & 0x80) !== 0);

  const v = (~(this.a ^ this.fetched) & (this.a ^ temp)) & 0x0080;
  this.setFlag(Flags6502.V, v !== 0);

  this.a = temp & 0x00FF;
  return 1;
}
```
:::

### SBC: Subtract with Borrow
Equation: $A = A - M - (1 - C)$

Using two's complement, $-M = \sim M + 1$. Subtraction becomes addition with the inverted operand:

$$A - M - (1 - C) = A + (\sim M) + C$$

::: code-group
```cpp [C++]
uint8_t CPU6502::SBC() {
    fetch();
    // Invert the bits of the operand
    uint16_t value = ((uint16_t)fetched) ^ 0x00FF;

    uint16_t temp = (uint16_t)a + value + (uint16_t)getFlag(C);

    // Carry flag is set if there is no borrow (result >= 0)
    setFlag(C, (temp & 0xFF00) > 0);
    setFlag(Z, (temp & 0x00FF) == 0);
    setFlag(N, (temp & 0x80) != 0);

    // Signed Overflow: V = (temp ^ A) & (temp ^ value) & 0x80
    setFlag(V, (temp ^ (uint16_t)a) & (temp ^ value) & 0x0080);

    a = temp & 0x00FF;
    return 1;
}
```

```rust [Rust]
pub fn sbc(&mut self) -> u8 {
    self.fetch();
    let value = (self.fetched as u16) ^ 0x00FF;
    let carry = if self.get_flag(Flag::C) { 1u16 } else { 0u16 };
    let temp = (self.a as u16) + value + carry;

    self.set_flag(Flag::C, (temp & 0xFF00) > 0);
    self.set_flag(Flag::Z, (temp & 0x00FF) == 0);
    self.set_flag(Flag::N, (temp & 0x80) != 0);

    let v = ((temp ^ (self.a as u16)) & (temp ^ value)) & 0x0080;
    self.set_flag(Flag::V, v != 0);

    self.a = (temp & 0x00FF) as u8;
    1
}
```

```typescript [TypeScript]
public SBC(): number {
  this.fetch();
  const value = (this.fetched ^ 0x00FF) & 0xFFFF;
  const temp = this.a + value + this.getFlag(Flags6502.C);

  this.setFlag(Flags6502.C, (temp & 0xFF00) > 0);
  this.setFlag(Flags6502.Z, (temp & 0x00FF) === 0);
  this.setFlag(Flags6502.N, (temp & 0x80) !== 0);

  const v = ((temp ^ this.a) & (temp ^ value)) & 0x0080;
  this.setFlag(Flags6502.V, v !== 0);

  this.a = temp & 0x00FF;
  return 1;
}
```
:::

## Comparisons (CMP, CPX, CPY)

Comparisons subtract the operand from the register without altering the register value:

::: code-group
```cpp [C++]
uint8_t CPU6502::CMP() {
    fetch();
    uint16_t temp = (uint16_t)a - (uint16_t)fetched;
    setFlag(C, a >= fetched);
    setFlag(Z, (temp & 0x00FF) == 0);
    setFlag(N, (temp & 0x0080) != 0);
    return 1;
}

uint8_t CPU6502::CPX() {
    fetch();
    uint16_t temp = (uint16_t)x - (uint16_t)fetched;
    setFlag(C, x >= fetched);
    setFlag(Z, (temp & 0x00FF) == 0);
    setFlag(N, (temp & 0x0080) != 0);
    return 0;
}

uint8_t CPU6502::CPY() {
    fetch();
    uint16_t temp = (uint16_t)y - (uint16_t)fetched;
    setFlag(C, y >= fetched);
    setFlag(Z, (temp & 0x00FF) == 0);
    setFlag(N, (temp & 0x0080) != 0);
    return 0;
}
```

```rust [Rust]
pub fn cmp(&mut self) -> u8 {
    self.fetch();
    let temp = (self.a as u16).wrapping_sub(self.fetched as u16);
    self.set_flag(Flag::C, self.a >= self.fetched);
    self.set_flag(Flag::Z, (temp & 0x00FF) == 0);
    self.set_flag(Flag::N, (temp & 0x0080) != 0);
    1
}

pub fn cpx(&mut self) -> u8 {
    self.fetch();
    let temp = (self.x as u16).wrapping_sub(self.fetched as u16);
    self.set_flag(Flag::C, self.x >= self.fetched);
    self.set_flag(Flag::Z, (temp & 0x00FF) == 0);
    self.set_flag(Flag::N, (temp & 0x0080) != 0);
    0
}

pub fn cpy(&mut self) -> u8 {
    self.fetch();
    let temp = (self.y as u16).wrapping_sub(self.fetched as u16);
    self.set_flag(Flag::C, self.y >= self.fetched);
    self.set_flag(Flag::Z, (temp & 0x00FF) == 0);
    self.set_flag(Flag::N, (temp & 0x0080) != 0);
    0
}
```

```typescript [TypeScript]
public CMP(): number {
  this.fetch();
  const temp = (this.a - this.fetched) & 0xFFFF;
  this.setFlag(Flags6502.C, this.a >= this.fetched);
  this.setFlag(Flags6502.Z, (temp & 0x00FF) === 0);
  this.setFlag(Flags6502.N, (temp & 0x0080) !== 0);
  return 1;
}

public CPX(): number {
  this.fetch();
  const temp = (this.x - this.fetched) & 0xFFFF;
  this.setFlag(Flags6502.C, this.x >= this.fetched);
  this.setFlag(Flags6502.Z, (temp & 0x00FF) === 0);
  this.setFlag(Flags6502.N, (temp & 0x0080) !== 0);
  return 0;
}

public CPY(): number {
  this.fetch();
  const temp = (this.y - this.fetched) & 0xFFFF;
  this.setFlag(Flags6502.C, this.y >= this.fetched);
  this.setFlag(Flags6502.Z, (temp & 0x00FF) === 0);
  this.setFlag(Flags6502.N, (temp & 0x0080) !== 0);
  return 0;
}
```
:::
