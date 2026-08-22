---
title: 3. Addressing Mode Resolvers
description: Implementing and testing the 13 addressing mode resolvers for the 6502 microprocessor.
---

Addressing modes determine how an instruction calculates the target memory address or locates its operand. The MOS 6502 supports 13 distinct addressing modes.

## Addressing Modes Summary

| Mode | Syntax | Bytes | Base Cycles | Extra Cycle on Page Cross? |
| :--- | :--- | :---: | :---: | :---: |
| **Implied (IMP)** | `CLC`, `TAX` | 1 | 2 | No |
| **Accumulator (ACC)** | `ASL A`, `ROR A` | 1 | 2 | No |
| **Immediate (IMM)** | `LDA #$10` | 2 | 2 | No |
| **Zero Page (ZP0)** | `LDA $80` | 2 | 3 | No |
| **Zero Page, X (ZPX)** | `LDA $80,X` | 2 | 4 | No (wraps in page 0) |
| **Zero Page, Y (ZPY)** | `LDX $80,Y` | 2 | 4 | No (wraps in page 0) |
| **Relative (REL)** | `BEQ label` | 2 | 2 | +1 if taken, +1 if page cross |
| **Absolute (ABS)** | `LDA $1234` | 3 | 4 | No |
| **Absolute, X (ABX)** | `LDA $1234,X` | 3 | 4 | +1 on page cross |
| **Absolute, Y (ABY)** | `LDA $1234,Y` | 3 | 4 | +1 on page cross |
| **Indirect (IND)** | `JMP ($1234)` | 3 | 5 | No (has page wrap bug) |
| **Indexed Indirect (IZX)** | `LDA ($80,X)` | 2 | 6 | No (wraps in page 0) |
| **Indirect Indexed (IZY)** | `LDA ($80),Y` | 2 | 5 | +1 on page cross |

## Addressing State Variables

Your CPU needs working variables to store the resolved target address, branch relative offset, and fetched operand:

::: code-group
```cpp [C++]
uint16_t addr_abs = 0x0000; // Calculated effective memory address
uint16_t addr_rel = 0x0000; // Relative branch target address
uint8_t  fetched  = 0x00;   // Operand data retrieved for the ALU
```

```rust [Rust]
pub struct CPU6502<'a> {
    pub addr_abs: u16, // Calculated effective memory address
    pub addr_rel: u16, // Relative branch target address
    pub fetched: u8,   // Operand data retrieved for the ALU
    // ...
    pub bus: &'a mut Bus,
}
```

```typescript [TypeScript]
export class CPU6502 {
  public addrAbs: number = 0x0000; // Calculated effective memory address
  public addrRel: number = 0x0000; // Relative branch target address
  public fetched: number = 0x00;   // Operand data retrieved for the ALU
  // ...
}
```
:::

## Step 1: Direct & Immediate Addressing

::: code-group
```cpp [C++]
// Implied / Accumulator (IMP / ACC)
uint8_t CPU6502::IMP() {
    fetched = a;
    return 0;
}

// Immediate (IMM): Operand is the byte immediately following the opcode
uint8_t CPU6502::IMM() {
    addr_abs = pc++;
    return 0;
}

// Zero Page (ZP0): Operand resides at $00LL
uint8_t CPU6502::ZP0() {
    addr_abs = read(pc);
    pc++;
    addr_abs &= 0x00FF;
    return 0;
}

// Zero Page with X Offset (ZPX): Address is ($00LL + X) & $00FF
uint8_t CPU6502::ZPX() {
    addr_abs = (read(pc) + x) & 0x00FF;
    pc++;
    return 0;
}

// Zero Page with Y Offset (ZPY): Address is ($00LL + Y) & $00FF (used by LDX, STX)
uint8_t CPU6502::ZPY() {
    addr_abs = (read(pc) + y) & 0x00FF;
    pc++;
    return 0;
}
```

```rust [Rust]
// Implied / Accumulator (IMP / ACC)
pub fn imp(&mut self) -> u8 {
    self.fetched = self.a;
    0
}

// Immediate (IMM): Operand is the byte immediately following the opcode
pub fn imm(&mut self) -> u8 {
    self.addr_abs = self.pc;
    self.pc += 1;
    0
}

// Zero Page (ZP0): Operand resides at $00LL
pub fn zp0(&mut self) -> u8 {
    self.addr_abs = (self.read(self.pc) as u16) & 0x00FF;
    self.pc += 1;
    0
}

// Zero Page with X Offset (ZPX): Address is ($00LL + X) & $00FF
pub fn zpx(&mut self) -> u8 {
    self.addr_abs = ((self.read(self.pc) as u16) + (self.x as u16)) & 0x00FF;
    self.pc += 1;
    0
}

// Zero Page with Y Offset (ZPY): Address is ($00LL + Y) & $00FF (used by LDX, STX)
pub fn zpy(&mut self) -> u8 {
    self.addr_abs = ((self.read(self.pc) as u16) + (self.y as u16)) & 0x00FF;
    self.pc += 1;
    0
}
```

```typescript [TypeScript]
// Implied / Accumulator (IMP / ACC)
public IMP(): number {
  this.fetched = this.a;
  return 0;
}

// Immediate (IMM): Operand is the byte immediately following the opcode
public IMM(): number {
  this.addrAbs = this.pc++;
  return 0;
}

// Zero Page (ZP0): Operand resides at $00LL
public ZP0(): number {
  this.addrAbs = this.read(this.pc) & 0x00FF;
  this.pc++;
  return 0;
}

// Zero Page with X Offset (ZPX): Address is ($00LL + X) & $00FF
public ZPX(): number {
  this.addrAbs = (this.read(this.pc) + this.x) & 0x00FF;
  this.pc++;
  return 0;
}

// Zero Page with Y Offset (ZPY): Address is ($00LL + Y) & $00FF (used by LDX, STX)
public ZPY(): number {
  this.addrAbs = (this.read(this.pc) + this.y) & 0x00FF;
  this.pc++;
  return 0;
}
```
:::

## Step 2: Absolute & Indexed Addressing

In absolute addressing, 16-bit addresses are stored in **little-endian** format (low byte first, then high byte):

::: code-group
```cpp [C++]
// Absolute (ABS): Full 16-bit address $HHLL
uint8_t CPU6502::ABS() {
    uint16_t lo = read(pc++);
    uint16_t hi = read(pc++);
    addr_abs = (hi << 8) | lo;
    return 0;
}

// Absolute, X (ABX): ($HHLL + X). Returns 1 if page boundary crossed.
uint8_t CPU6502::ABX() {
    uint16_t lo = read(pc++);
    uint16_t hi = read(pc++);
    addr_abs = ((hi << 8) | lo) + x;

    // Check if adding X crossed the 256-byte page boundary
    if ((addr_abs & 0xFF00) != (hi << 8)) {
        return 1; // Extra cycle needed
    }
    return 0;
}

// Absolute, Y (ABY): ($HHLL + Y). Returns 1 if page boundary crossed.
uint8_t CPU6502::ABY() {
    uint16_t lo = read(pc++);
    uint16_t hi = read(pc++);
    addr_abs = ((hi << 8) | lo) + y;

    if ((addr_abs & 0xFF00) != (hi << 8)) {
        return 1; // Extra cycle needed
    }
    return 0;
}
```

```rust [Rust]
// Absolute (ABS): Full 16-bit address $HHLL
pub fn abs(&mut self) -> u8 {
    let lo = self.read(self.pc) as u16;
    self.pc += 1;
    let hi = self.read(self.pc) as u16;
    self.pc += 1;
    self.addr_abs = (hi << 8) | lo;
    0
}

// Absolute, X (ABX): ($HHLL + X). Returns 1 if page boundary crossed.
pub fn abx(&mut self) -> u8 {
    let lo = self.read(self.pc) as u16;
    self.pc += 1;
    let hi = self.read(self.pc) as u16;
    self.pc += 1;
    self.addr_abs = ((hi << 8) | lo).wrapping_add(self.x as u16);

    if (self.addr_abs & 0xFF00) != (hi << 8) {
        1
    } else {
        0
    }
}

// Absolute, Y (ABY): ($HHLL + Y). Returns 1 if page boundary crossed.
pub fn aby(&mut self) -> u8 {
    let lo = self.read(self.pc) as u16;
    self.pc += 1;
    let hi = self.read(self.pc) as u16;
    self.pc += 1;
    self.addr_abs = ((hi << 8) | lo).wrapping_add(self.y as u16);

    if (self.addr_abs & 0xFF00) != (hi << 8) {
        1
    } else {
        0
    }
}
```

```typescript [TypeScript]
// Absolute (ABS): Full 16-bit address $HHLL
public ABS(): number {
  const lo = this.read(this.pc++);
  const hi = this.read(this.pc++);
  this.addrAbs = (hi << 8) | lo;
  return 0;
}

// Absolute, X (ABX): ($HHLL + X). Returns 1 if page boundary crossed.
public ABX(): number {
  const lo = this.read(this.pc++);
  const hi = this.read(this.pc++);
  this.addrAbs = (((hi << 8) | lo) + this.x) & 0xFFFF;

  if ((this.addrAbs & 0xFF00) !== (hi << 8)) {
    return 1; // Extra cycle needed
  }
  return 0;
}

// Absolute, Y (ABY): ($HHLL + Y). Returns 1 if page boundary crossed.
public ABY(): number {
  const lo = this.read(this.pc++);
  const hi = this.read(this.pc++);
  this.addrAbs = (((hi << 8) | lo) + this.y) & 0xFFFF;

  if ((this.addrAbs & 0xFF00) !== (hi << 8)) {
    return 1; // Extra cycle needed
  }
  return 0;
}
```
:::

## Step 3: Relative Branch Addressing

Relative mode takes a signed 8-bit offset ($-128$ to $+127$):

::: code-group
```cpp [C++]
uint8_t CPU6502::REL() {
    addr_rel = read(pc++);
    // Sign-extend 8-bit negative numbers to 16 bits
    if (addr_rel & 0x80) {
        addr_rel |= 0xFF00;
    }
    return 0;
}
```

```rust [Rust]
pub fn rel(&mut self) -> u8 {
    self.addr_rel = self.read(self.pc) as u16;
    self.pc += 1;
    if (self.addr_rel & 0x80) != 0 {
        self.addr_rel |= 0xFF00;
    }
    0
}
```

```typescript [TypeScript]
public REL(): number {
  this.addrRel = this.read(this.pc++);
  // Sign-extend 8-bit negative numbers to 16 bits
  if (this.addrRel & 0x80) {
    this.addrRel |= 0xFF00;
  }
  return 0;
}
```
:::

## Step 4: Indirect Addressing Modes

### 1. Indirect (IND) — `JMP ($xxxx)`
Reads a 16-bit pointer from memory. It contains a famous hardware bug on NMOS 6502: if the low byte is `$FF`, the high byte wraps within the same page rather than advancing to the next page!

::: code-group
```cpp [C++]
uint8_t CPU6502::IND() {
    uint16_t ptr_lo = read(pc++);
    uint16_t ptr_hi = read(pc++);
    uint16_t ptr = (ptr_hi << 8) | ptr_lo;

    if (ptr_lo == 0x00FF) {
        // Hardware bug: fetches hi-byte from $xx00 instead of $xx00 + 0x100
        addr_abs = (read(ptr & 0xFF00) << 8) | read(ptr);
    } else {
        addr_abs = (read(ptr + 1) << 8) | read(ptr);
    }
    return 0;
}
```

```rust [Rust]
pub fn ind(&mut self) -> u8 {
    let ptr_lo = self.read(self.pc) as u16;
    self.pc += 1;
    let ptr_hi = self.read(self.pc) as u16;
    self.pc += 1;
    let ptr = (ptr_hi << 8) | ptr_lo;

    if ptr_lo == 0x00FF {
        self.addr_abs = ((self.read(ptr & 0xFF00) as u16) << 8) | (self.read(ptr) as u16);
    } else {
        self.addr_abs = ((self.read(ptr + 1) as u16) << 8) | (self.read(ptr) as u16);
    }
    0
}
```

```typescript [TypeScript]
public IND(): number {
  const ptrLo = this.read(this.pc++);
  const ptrHi = this.read(this.pc++);
  const ptr = (ptrHi << 8) | ptrLo;

  if (ptrLo === 0x00FF) {
    this.addrAbs = (this.read(ptr & 0xFF00) << 8) | this.read(ptr);
  } else {
    this.addrAbs = (this.read(ptr + 1) << 8) | this.read(ptr);
  }
  return 0;
}
```
:::

### 2. Indexed Indirect (IZX) — `($LL, X)`
Also called "Indirect, X" or "Pre-indexed indirect". Adds `X` to the zero page pointer base (with zero page wrap), then reads the 16-bit address from `$00(base+X)`:

::: code-group
```cpp [C++]
uint8_t CPU6502::IZX() {
    uint16_t t = read(pc++);
    uint16_t lo = read((t + (uint16_t)x) & 0x00FF);
    uint16_t hi = read((t + (uint16_t)x + 1) & 0x00FF);
    addr_abs = (hi << 8) | lo;
    return 0;
}
```

```rust [Rust]
pub fn izx(&mut self) -> u8 {
    let t = self.read(self.pc) as u16;
    self.pc += 1;
    let lo = self.read((t + (self.x as u16)) & 0x00FF) as u16;
    let hi = self.read((t + (self.x as u16) + 1) & 0x00FF) as u16;
    self.addr_abs = (hi << 8) | lo;
    0
}
```

```typescript [TypeScript]
public IZX(): number {
  const t = this.read(this.pc++);
  const lo = this.read((t + this.x) & 0x00FF);
  const hi = this.read((t + this.x + 1) & 0x00FF);
  this.addrAbs = (hi << 8) | lo;
  return 0;
}
```
:::

### 3. Indirect Indexed (IZY) — `($LL), Y`
Also called "Indirect, Y" or "Post-indexed indirect". Reads the 16-bit address from `$00LL`, and *then* adds `Y` to it. Returns 1 if adding `Y` crosses a page boundary:

::: code-group
```cpp [C++]
uint8_t CPU6502::IZY() {
    uint16_t t = read(pc++);
    uint16_t lo = read(t & 0x00FF);
    uint16_t hi = read((t + 1) & 0x00FF);

    addr_abs = ((hi << 8) | lo) + y;
    if ((addr_abs & 0xFF00) != (hi << 8)) {
        return 1; // Extra cycle needed
    }
    return 0;
}
```

```rust [Rust]
pub fn izy(&mut self) -> u8 {
    let t = self.read(self.pc) as u16;
    self.pc += 1;
    let lo = self.read(t & 0x00FF) as u16;
    let hi = self.read((t + 1) & 0x00FF) as u16;

    self.addr_abs = ((hi << 8) | lo).wrapping_add(self.y as u16);
    if (self.addr_abs & 0xFF00) != (hi << 8) {
        1
    } else {
        0
    }
}
```

```typescript [TypeScript]
public IZY(): number {
  const t = this.read(this.pc++);
  const lo = this.read(t & 0x00FF);
  const hi = this.read((t + 1) & 0x00FF);

  this.addrAbs = (((hi << 8) | lo) + this.y) & 0xFFFF;
  if ((this.addrAbs & 0xFF00) !== (hi << 8)) {
    return 1; // Extra cycle needed
  }
  return 0;
}
```
:::

## Operand Fetch Helper

Create a universal `fetch()` method that reads data from `addr_abs` unless the mode is `IMP`:

::: code-group
```cpp [C++]
uint8_t CPU6502::fetch() {
    if (!(lookup[opcode].addrmode == &CPU6502::IMP)) {
        fetched = read(addr_abs);
    }
    return fetched;
}
```

```rust [Rust]
pub fn fetch(&mut self) -> u8 {
    // In Rust, check if the current opcode uses implied/accumulator mode
    if self.lookup[self.opcode as usize].addrmode != AddrMode::IMP {
        self.fetched = self.read(self.addr_abs);
    }
    self.fetched
}
```

```typescript [TypeScript]
public fetch(): number {
  if (this.lookup[this.opcode].addrmode !== this.IMP) {
    this.fetched = this.read(this.addrAbs);
  }
  return this.fetched;
}
```
:::
