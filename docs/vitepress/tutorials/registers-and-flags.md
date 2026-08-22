---
title: 2. Register File & Status Flags
description: Building the 6502 register file and implementing status register flag bit manipulation.
---

The 6502 has a compact set of architectural registers. Modeling their exact widths and flag behavior is essential for executing instructions correctly.

## Register Layout

| Register | Width | Purpose | Initial / Reset State |
| :--- | :--- | :--- | :--- |
| **A (Accumulator)** | 8-bit | Arithmetic, logic operations, ALU operand & result destination | `0x00` |
| **X (X Index)** | 8-bit | Indexing, loop counters, stack pointer transfer | `0x00` |
| **Y (Y Index)** | 8-bit | Indexing, loop counters, indirect addressing | `0x00` |
| **PC (Program Counter)** | 16-bit | Points to the next opcode or operand to fetch | Loaded from `$FFFC/$FFFD` |
| **SP (Stack Pointer)** | 8-bit | Points to current top of stack in Page 1 (`$0100-$01FF`) | `0xFD` (on reset) |
| **P / SR (Status Register)** | 8-bit | 7 condition and control flags + 1 unused bit | `0x24` or `0x34` (Unused bit 5 = 1) |

## The Status Register (P / SR)

The status register contains condition flags updated by ALU operations and control flags that configure CPU behavior:

```text
 Bit:   7   6   5   4   3   2   1   0
 Flag:  N   V   U   B   D   I   Z   C
        │   │   │   │   │   │   │   └── Carry Flag (C)
        │   │   │   │   │   │   └────── Zero Flag (Z)
        │   │   │   │   │   └────────── Interrupt Disable (I)
        │   │   │   │   └────────────── Decimal Mode (D)
        │   │   │   └────────────────── Break Flag (B, only on stack)
        │   │   └────────────────────── Unused (U, always pushed as 1)
        │   └────────────────────────── Overflow Flag (V)
        └────────────────────────────── Negative Flag (N)
```

## Step 1: Defining Status Flag Enums

::: code-group
```cpp [C++]
enum Flags6502 : uint8_t {
    C = (1 << 0), // Carry (1 = Carry / No Borrow)
    Z = (1 << 1), // Zero (1 = Result is 0)
    I = (1 << 2), // Interrupt Disable (1 = Mask IRQ)
    D = (1 << 3), // Decimal Mode (1 = BCD mode)
    B = (1 << 4), // Break Command
    U = (1 << 5), // Unused (always 1 when pushed)
    V = (1 << 6), // Overflow (signed arithmetic overflow)
    N = (1 << 7), // Negative (1 = Bit 7 is set)
};
```

```rust [Rust]
#[repr(u8)]
pub enum Flag {
    C = 1 << 0,
    Z = 1 << 1,
    I = 1 << 2,
    D = 1 << 3,
    B = 1 << 4,
    U = 1 << 5,
    V = 1 << 6,
    N = 1 << 7,
}
```

```typescript [TypeScript]
export enum Flags6502 {
  C = 1 << 0, // Carry (1 = Carry / No Borrow)
  Z = 1 << 1, // Zero (1 = Result is 0)
  I = 1 << 2, // Interrupt Disable (1 = Mask IRQ)
  D = 1 << 3, // Decimal Mode (1 = BCD mode)
  B = 1 << 4, // Break Command
  U = 1 << 5, // Unused (always 1 when pushed)
  V = 1 << 6, // Overflow (signed arithmetic overflow)
  N = 1 << 7, // Negative (1 = Bit 7 is set)
}
```
:::

## Step 2: Flag Access Helpers

Add helper methods to your CPU class to read and modify status flags efficiently:

::: code-group
```cpp [C++]
uint8_t getFlag(Flags6502 f) const {
    return ((status & f) > 0) ? 1 : 0;
}

void setFlag(Flags6502 f, bool v) {
    if (v) {
        status |= f;
    } else {
        status &= ~f;
    }
}

// Common helper for instructions updating N and Z flags (LDA, LDX, LDY, TAX, INX, etc.)
void updateNZ(uint8_t value) {
    setFlag(Z, value == 0x00);
    setFlag(N, (value & 0x80) != 0);
}
```

```rust [Rust]
impl<'a> CPU6502<'a> {
    pub fn get_flag(&self, flag: Flag) -> bool {
        (self.status & (flag as u8)) != 0
    }

    pub fn set_flag(&mut self, flag: Flag, val: bool) {
        if val {
            self.status |= flag as u8;
        } else {
            self.status &= !(flag as u8);
        }
    }

    pub fn update_nz(&mut self, val: u8) {
        self.set_flag(Flag::Z, val == 0);
        self.set_flag(Flag::N, (val & 0x80) != 0);
    }
}
```

```typescript [TypeScript]
export class CPU6502 {
  public status: number = 0x00 | Flags6502.U;

  public getFlag(f: Flags6502): number {
    return (this.status & f) > 0 ? 1 : 0;
  }

  public setFlag(f: Flags6502, v: boolean): void {
    if (v) {
      this.status |= f;
    } else {
      this.status &= ~f;
    }
  }

  public updateNZ(value: number): void {
    this.setFlag(Flags6502.Z, (value & 0xFF) === 0x00);
    this.setFlag(Flags6502.N, (value & 0x80) !== 0);
  }
}
```
:::

## Step 3: Register File Class

Assemble the complete register state:

::: code-group
```cpp [C++]
class CPU6502 {
public:
    uint8_t  a      = 0x00; // Accumulator
    uint8_t  x      = 0x00; // X Register
    uint8_t  y      = 0x00; // Y Register
    uint8_t  stkp   = 0xFD; // Stack Pointer
    uint16_t pc     = 0x0000; // Program Counter
    uint8_t  status = 0x00 | Flags6502::U; // Status Register

    // ...
};
```

```rust [Rust]
pub struct CPU6502<'a> {
    pub a: u8,        // Accumulator
    pub x: u8,        // X Register
    pub y: u8,        // Y Register
    pub stkp: u8,     // Stack Pointer
    pub pc: u16,      // Program Counter
    pub status: u8,   // Status Register
    pub bus: &'a mut Bus,
}

impl<'a> CPU6502<'a> {
    pub fn new(bus: &'a mut Bus) -> Self {
        Self {
            a: 0x00,
            x: 0x00,
            y: 0x00,
            stkp: 0xFD,
            pc: 0x0000,
            status: 0x00 | (Flag::U as u8),
            bus,
        }
    }
}
```

```typescript [TypeScript]
export class CPU6502 {
  public a: number = 0x00;       // Accumulator
  public x: number = 0x00;       // X Register
  public y: number = 0x00;       // Y Register
  public stkp: number = 0xFD;    // Stack Pointer
  public pc: number = 0x0000;    // Program Counter
  public status: number = 0x00 | Flags6502.U; // Status Register
  private bus: Bus | null = null;
}
```
:::
