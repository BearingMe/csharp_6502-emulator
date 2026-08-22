---
title: How to Implement Bitwise Shifts and Rotates
description: Practical recipes for emulating ASL, LSR, ROL, ROR, and BIT instructions on accumulator and memory operands.
---

The 6502 provides bitwise shift and rotate instructions that can target either the Accumulator directly or a memory address.

## Shift & Rotate Operations Overview

```text
ASL (Arithmetic Shift Left):
C <── [ Bit 7 | Bit 6 | Bit 5 | Bit 4 | Bit 3 | Bit 2 | Bit 1 | Bit 0 ] <── 0

LSR (Logical Shift Right):
0 ──> [ Bit 7 | Bit 6 | Bit 5 | Bit 4 | Bit 3 | Bit 2 | Bit 1 | Bit 0 ] ──> C

ROL (Rotate Left through Carry):
C <── [ Bit 7 | Bit 6 | Bit 5 | Bit 4 | Bit 3 | Bit 2 | Bit 1 | Bit 0 ] <── C_in

ROR (Rotate Right through Carry):
C_in ──> [ Bit 7 | Bit 6 | Bit 5 | Bit 4 | Bit 3 | Bit 2 | Bit 1 | Bit 0 ] ──> C
```

---

## 1. Implementing `ASL` (Arithmetic Shift Left)

- Shifts all bits left by 1 position.
- Bit 7 moves into the **Carry (C)** flag.
- Bit 0 is loaded with `0`.

::: code-group
```cpp [C++]
uint8_t CPU6502::ASL() {
    fetch();
    uint16_t temp = (uint16_t)fetched << 1;

    setFlag(Flags6502::C, (temp & 0xFF00) > 0);
    setFlag(Flags6502::Z, (temp & 0x00FF) == 0);
    setFlag(Flags6502::N, (temp & 0x80) != 0);

    if (lookup[opcode].addrmode == &CPU6502::IMP) {
        a = temp & 0x00FF;
    } else {
        write(addr_abs, temp & 0x00FF);
    }
    return 0;
}
```

```rust [Rust]
pub fn asl(&mut self) -> u8 {
    self.fetch();
    let temp = (self.fetched as u16) << 1;

    self.set_flag(Flag::C, (temp & 0xFF00) > 0);
    self.set_flag(Flag::Z, (temp & 0x00FF) == 0);
    self.set_flag(Flag::N, (temp & 0x80) != 0);

    if self.lookup[self.opcode as usize].addrmode == AddrMode::IMP {
        self.a = (temp & 0x00FF) as u8;
    } else {
        self.write(self.addr_abs, (temp & 0x00FF) as u8);
    }
    0
}
```

```typescript [TypeScript]
public ASL(): number {
  this.fetch();
  const temp = (this.fetched << 1) & 0xFFFF;

  this.setFlag(Flags6502.C, (temp & 0xFF00) > 0);
  this.setFlag(Flags6502.Z, (temp & 0x00FF) === 0);
  this.setFlag(Flags6502.N, (temp & 0x80) !== 0);

  if (this.lookup[this.opcode].addrmode === this.IMP) {
    this.a = temp & 0x00FF;
  } else {
    this.write(this.addrAbs, temp & 0x00FF);
  }
  return 0;
}
```
:::

---

## 2. Implementing `LSR` (Logical Shift Right)

- Shifts all bits right by 1 position.
- Bit 0 moves into the **Carry (C)** flag.
- Bit 7 is loaded with `0` (hence `N` is always cleared).

::: code-group
```cpp [C++]
uint8_t CPU6502::LSR() {
    fetch();
    setFlag(Flags6502::C, (fetched & 0x01) != 0);

    uint8_t temp = fetched >> 1;
    setFlag(Flags6502::Z, temp == 0);
    setFlag(Flags6502::N, false);

    if (lookup[opcode].addrmode == &CPU6502::IMP) {
        a = temp;
    } else {
        write(addr_abs, temp);
    }
    return 0;
}
```

```rust [Rust]
pub fn lsr(&mut self) -> u8 {
    self.fetch();
    self.set_flag(Flag::C, (self.fetched & 0x01) != 0);

    let temp = self.fetched >> 1;
    self.set_flag(Flag::Z, temp == 0);
    self.set_flag(Flag::N, false);

    if self.lookup[self.opcode as usize].addrmode == AddrMode::IMP {
        self.a = temp;
    } else {
        self.write(self.addr_abs, temp);
    }
    0
}
```

```typescript [TypeScript]
public LSR(): number {
  this.fetch();
  this.setFlag(Flags6502.C, (this.fetched & 0x01) !== 0);

  const temp = (this.fetched >> 1) & 0xFF;
  this.setFlag(Flags6502.Z, temp === 0);
  this.setFlag(Flags6502.N, false);

  if (this.lookup[this.opcode].addrmode === this.IMP) {
    this.a = temp;
  } else {
    this.write(this.addrAbs, temp);
  }
  return 0;
}
```
:::

---

## 3. Implementing `ROL` (Rotate Left)

- Shifts all bits left by 1.
- Bit 7 moves into the **Carry (C)** flag.
- The previous value of Carry enters at **Bit 0**.

::: code-group
```cpp [C++]
uint8_t CPU6502::ROL() {
    fetch();
    uint16_t temp = ((uint16_t)fetched << 1) | getFlag(Flags6502::C);

    setFlag(Flags6502::C, (temp & 0xFF00) > 0);
    setFlag(Flags6502::Z, (temp & 0x00FF) == 0);
    setFlag(Flags6502::N, (temp & 0x80) != 0);

    if (lookup[opcode].addrmode == &CPU6502::IMP) {
        a = temp & 0x00FF;
    } else {
        write(addr_abs, temp & 0x00FF);
    }
    return 0;
}
```

```rust [Rust]
pub fn rol(&mut self) -> u8 {
    self.fetch();
    let carry = if self.get_flag(Flag::C) { 1u16 } else { 0u16 };
    let temp = ((self.fetched as u16) << 1) | carry;

    self.set_flag(Flag::C, (temp & 0xFF00) > 0);
    self.set_flag(Flag::Z, (temp & 0x00FF) == 0);
    self.set_flag(Flag::N, (temp & 0x80) != 0);

    if self.lookup[self.opcode as usize].addrmode == AddrMode::IMP {
        self.a = (temp & 0x00FF) as u8;
    } else {
        self.write(self.addr_abs, (temp & 0x00FF) as u8);
    }
    0
}
```

```typescript [TypeScript]
public ROL(): number {
  this.fetch();
  const temp = ((this.fetched << 1) | this.getFlag(Flags6502.C)) & 0xFFFF;

  this.setFlag(Flags6502.C, (temp & 0xFF00) > 0);
  this.setFlag(Flags6502.Z, (temp & 0x00FF) === 0);
  this.setFlag(Flags6502.N, (temp & 0x80) !== 0);

  if (this.lookup[this.opcode].addrmode === this.IMP) {
    this.a = temp & 0x00FF;
  } else {
    this.write(this.addrAbs, temp & 0x00FF);
  }
  return 0;
}
```
:::

---

## 4. Implementing `ROR` (Rotate Right)

- Shifts all bits right by 1.
- Bit 0 moves into the **Carry (C)** flag.
- The previous value of Carry enters at **Bit 7**.

::: code-group
```cpp [C++]
uint8_t CPU6502::ROR() {
    fetch();
    uint8_t old_carry = getFlag(Flags6502::C);
    setFlag(Flags6502::C, (fetched & 0x01) != 0);

    uint8_t temp = (old_carry << 7) | (fetched >> 1);
    setFlag(Flags6502::Z, temp == 0);
    setFlag(Flags6502::N, (temp & 0x80) != 0);

    if (lookup[opcode].addrmode == &CPU6502::IMP) {
        a = temp;
    } else {
        write(addr_abs, temp);
    }
    return 0;
}
```

```rust [Rust]
pub fn ror(&mut self) -> u8 {
    self.fetch();
    let old_carry = if self.get_flag(Flag::C) { 1u8 } else { 0u8 };
    self.set_flag(Flag::C, (self.fetched & 0x01) != 0);

    let temp = (old_carry << 7) | (self.fetched >> 1);
    self.set_flag(Flag::Z, temp == 0);
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
public ROR(): number {
  this.fetch();
  const oldCarry = this.getFlag(Flags6502.C);
  this.setFlag(Flags6502.C, (this.fetched & 0x01) !== 0);

  const temp = ((oldCarry << 7) | (this.fetched >> 1)) & 0xFF;
  this.setFlag(Flags6502.Z, temp === 0);
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

---

## 5. Implementing `BIT` (Bit Test)

- Does **not** modify the Accumulator `A`.
- Performs `A & M` to update the `Z` flag.
- Copies bit 7 of `M` directly into `N`.
- Copies bit 6 of `M` directly into `V`.

::: code-group
```cpp [C++]
uint8_t CPU6502::BIT() {
    fetch();
    uint8_t test = a & fetched;
    setFlag(Flags6502::Z, test == 0);
    setFlag(Flags6502::N, (fetched & (1 << 7)) != 0);
    setFlag(Flags6502::V, (fetched & (1 << 6)) != 0);
    return 0;
}
```

```rust [Rust]
pub fn bit(&mut self) -> u8 {
    self.fetch();
    let test = self.a & self.fetched;
    self.set_flag(Flag::Z, test == 0);
    self.set_flag(Flag::N, (self.fetched & (1 << 7)) != 0);
    self.set_flag(Flag::V, (self.fetched & (1 << 6)) != 0);
    0
}
```

```typescript [TypeScript]
public BIT(): number {
  this.fetch();
  const test = (this.a & this.fetched) & 0xFF;
  this.setFlag(Flags6502.Z, test === 0);
  this.setFlag(Flags6502.N, (this.fetched & (1 << 7)) !== 0);
  this.setFlag(Flags6502.V, (this.fetched & (1 << 6)) !== 0);
  return 0;
}
```
:::
