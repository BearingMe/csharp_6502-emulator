---
title: 6. Interrupts & System Vectors
description: Implementing CPU reset sequence, Maskable Interrupts (IRQ), Non-Maskable Interrupts (NMI), and Software Break (BRK).
---

The 6502 supports three hardware interrupt pins (**RES**, **NMI**, **IRQ**) and one software interrupt instruction (**BRK**). Each routes execution to a dedicated 16-bit vector located at the top of memory.

## Hardware Interrupt Vectors

| Vector Address | Vector Type | Maskable by `I` Flag? | Stack Pushed Values |
| :--- | :--- | :---: | :--- |
| `$FFFA / $FFFB` | **NMI** (Non-Maskable Interrupt) | No | `PC_hi`, `PC_lo`, `Status` (with `B=0`, `U=1`) |
| `$FFFC / $FFFD` | **RESET** | No | None (Registers initialized, `PC` loaded) |
| `$FFFE / $FFFF` | **IRQ** / **BRK** | Yes (IRQ only) | `PC_hi`, `PC_lo`, `Status` (`B=0` for IRQ, `B=1` for BRK) |

## Step 1: System Reset (`reset()`)

When reset is asserted:
1. Registers `A`, `X`, `Y` are initialized to `0x00`.
2. Stack Pointer `SP` is set to `0xFD`.
3. Status Register `status` is set to `0x00 | U` (`0x20` or with `I` set `0x24`).
4. Program Counter `PC` is loaded with the 16-bit address at `$FFFC` (little-endian).
5. Reset sequence consumes 8 CPU cycles.

::: code-group
```cpp [C++]
void CPU6502::reset() {
    addr_abs = 0xFFFC;
    uint16_t lo = read(addr_abs + 0);
    uint16_t hi = read(addr_abs + 1);
    pc = (hi << 8) | lo;

    a = 0;
    x = 0;
    y = 0;
    stkp = 0xFD;
    status = 0x00 | Flags6502::U | Flags6502::I;

    addr_abs = 0;
    addr_rel = 0;
    fetched = 0;

    cycles = 8;
}
```

```rust [Rust]
pub fn reset(&mut self) {
    self.addr_abs = 0xFFFC;
    let lo = self.read(self.addr_abs) as u16;
    let hi = self.read(self.addr_abs + 1) as u16;
    self.pc = (hi << 8) | lo;

    self.a = 0;
    self.x = 0;
    self.y = 0;
    self.stkp = 0xFD;
    self.status = (Flag::U as u8) | (Flag::I as u8);

    self.addr_abs = 0;
    self.addr_rel = 0;
    self.fetched = 0;

    self.cycles = 8;
}
```

```typescript [TypeScript]
public reset(): void {
  this.addrAbs = 0xFFFC;
  const lo = this.read(this.addrAbs);
  const hi = this.read(this.addrAbs + 1);
  this.pc = (hi << 8) | lo;

  this.a = 0;
  this.x = 0;
  this.y = 0;
  this.stkp = 0xFD;
  this.status = Flags6502.U | Flags6502.I;

  this.addrAbs = 0;
  this.addrRel = 0;
  this.fetched = 0;

  this.cycles = 8;
}
```
:::

## Step 2: Interrupt Request (`irq()`)

Triggered by external hardware. Executes only when Interrupt Disable `I` flag is `0`:
1. Push `PC` (high byte first, then low byte) onto stack at `$0100 + SP`.
2. Push `Status` register onto stack with `B=0`, `U=1`, `I=1`.
3. Set `I` flag in CPU to prevent nested interrupts.
4. Load `PC` from 16-bit vector at `$FFFE`.
5. Takes 7 clock cycles.

::: code-group
```cpp [C++]
void CPU6502::irq() {
    if (getFlag(Flags6502::I) == 0) {
        // Push PC high byte
        write(0x0100 + stkp, (pc >> 8) & 0x00FF);
        stkp--;
        // Push PC low byte
        write(0x0100 + stkp, pc & 0x00FF);
        stkp--;

        // Push Status with B=0, U=1, I=1
        setFlag(Flags6502::B, 0);
        setFlag(Flags6502::U, 1);
        setFlag(Flags6502::I, 1);
        write(0x0100 + stkp, status);
        stkp--;

        // Read IRQ vector
        uint16_t lo = read(0xFFFE);
        uint16_t hi = read(0xFFFF);
        pc = (hi << 8) | lo;

        cycles = 7;
    }
}
```

```rust [Rust]
pub fn irq(&mut self) {
    if !self.get_flag(Flag::I) {
        self.write(0x0100 + (self.stkp as u16), ((self.pc >> 8) & 0x00FF) as u8);
        self.stkp = self.stkp.wrapping_sub(1);
        self.write(0x0100 + (self.stkp as u16), (self.pc & 0x00FF) as u8);
        self.stkp = self.stkp.wrapping_sub(1);

        self.set_flag(Flag::B, false);
        self.set_flag(Flag::U, true);
        self.set_flag(Flag::I, true);
        self.write(0x0100 + (self.stkp as u16), self.status);
        self.stkp = self.stkp.wrapping_sub(1);

        let lo = self.read(0xFFFE) as u16;
        let hi = self.read(0xFFFF) as u16;
        self.pc = (hi << 8) | lo;

        self.cycles = 7;
    }
}
```

```typescript [TypeScript]
public irq(): void {
  if (this.getFlag(Flags6502.I) === 0) {
    this.write(0x0100 + this.stkp, (this.pc >> 8) & 0x00FF);
    this.stkp = (this.stkp - 1) & 0xFF;
    this.write(0x0100 + this.stkp, this.pc & 0x00FF);
    this.stkp = (this.stkp - 1) & 0xFF;

    this.setFlag(Flags6502.B, false);
    this.setFlag(Flags6502.U, true);
    this.setFlag(Flags6502.I, true);
    this.write(0x0100 + this.stkp, this.status);
    this.stkp = (this.stkp - 1) & 0xFF;

    const lo = this.read(0xFFFE);
    const hi = this.read(0xFFFF);
    this.pc = (hi << 8) | lo;

    this.cycles = 7;
  }
}
```
:::

## Step 3: Non-Maskable Interrupt (`nmi()`)

Unconditional interrupt triggered regardless of the `I` flag:

::: code-group
```cpp [C++]
void CPU6502::nmi() {
    write(0x0100 + stkp, (pc >> 8) & 0x00FF);
    stkp--;
    write(0x0100 + stkp, pc & 0x00FF);
    stkp--;

    setFlag(Flags6502::B, 0);
    setFlag(Flags6502::U, 1);
    setFlag(Flags6502::I, 1);
    write(0x0100 + stkp, status);
    stkp--;

    uint16_t lo = read(0xFFFA);
    uint16_t hi = read(0xFFFB);
    pc = (hi << 8) | lo;

    cycles = 8;
}
```

```rust [Rust]
pub fn nmi(&mut self) {
    self.write(0x0100 + (self.stkp as u16), ((self.pc >> 8) & 0x00FF) as u8);
    self.stkp = self.stkp.wrapping_sub(1);
    self.write(0x0100 + (self.stkp as u16), (self.pc & 0x00FF) as u8);
    self.stkp = self.stkp.wrapping_sub(1);

    self.set_flag(Flag::B, false);
    self.set_flag(Flag::U, true);
    self.set_flag(Flag::I, true);
    self.write(0x0100 + (self.stkp as u16), self.status);
    self.stkp = self.stkp.wrapping_sub(1);

    let lo = self.read(0xFFFA) as u16;
    let hi = self.read(0xFFFB) as u16;
    self.pc = (hi << 8) | lo;

    self.cycles = 8;
}
```

```typescript [TypeScript]
public nmi(): void {
  this.write(0x0100 + this.stkp, (this.pc >> 8) & 0x00FF);
  this.stkp = (this.stkp - 1) & 0xFF;
  this.write(0x0100 + this.stkp, this.pc & 0x00FF);
  this.stkp = (this.stkp - 1) & 0xFF;

  this.setFlag(Flags6502.B, false);
  this.setFlag(Flags6502.U, true);
  this.setFlag(Flags6502.I, true);
  this.write(0x0100 + this.stkp, this.status);
  this.stkp = (this.stkp - 1) & 0xFF;

  const lo = this.read(0xFFFA);
  const hi = this.read(0xFFFB);
  this.pc = (hi << 8) | lo;

  this.cycles = 8;
}
```
:::

## Step 4: Software Break (`BRK`)

The `BRK` instruction forces an interrupt from software:
- Pushes `PC + 1` (since `PC` was already incremented past the opcode byte)
- Pushes `status` with **Break flag `B = 1`**
- Loads `PC` from `$FFFE`

::: code-group
```cpp [C++]
uint8_t CPU6502::BRK() {
    pc++; // BRK advances PC past a padding byte

    setFlag(Flags6502::I, 1);
    write(0x0100 + stkp, (pc >> 8) & 0x00FF);
    stkp--;
    write(0x0100 + stkp, pc & 0x00FF);
    stkp--;

    // Push status with Break bit set
    setFlag(Flags6502::B, 1);
    write(0x0100 + stkp, status);
    stkp--;
    setFlag(Flags6502::B, 0);

    pc = (uint16_t)read(0xFFFE) | ((uint16_t)read(0xFFFF) << 8);
    return 0;
}
```

```rust [Rust]
pub fn brk(&mut self) -> u8 {
    self.pc += 1; // BRK advances PC past a padding byte

    self.set_flag(Flag::I, true);
    self.write(0x0100 + (self.stkp as u16), ((self.pc >> 8) & 0x00FF) as u8);
    self.stkp = self.stkp.wrapping_sub(1);
    self.write(0x0100 + (self.stkp as u16), (self.pc & 0x00FF) as u8);
    self.stkp = self.stkp.wrapping_sub(1);

    self.set_flag(Flag::B, true);
    self.write(0x0100 + (self.stkp as u16), self.status);
    self.stkp = self.stkp.wrapping_sub(1);
    self.set_flag(Flag::B, false);

    let lo = self.read(0xFFFE) as u16;
    let hi = self.read(0xFFFF) as u16;
    self.pc = (hi << 8) | lo;
    0
}
```

```typescript [TypeScript]
public BRK(): number {
  this.pc++; // BRK advances PC past a padding byte

  this.setFlag(Flags6502.I, true);
  this.write(0x0100 + this.stkp, (this.pc >> 8) & 0x00FF);
  this.stkp = (this.stkp - 1) & 0xFF;
  this.write(0x0100 + this.stkp, this.pc & 0x00FF);
  this.stkp = (this.stkp - 1) & 0xFF;

  this.setFlag(Flags6502.B, true);
  this.write(0x0100 + this.stkp, this.status);
  this.stkp = (this.stkp - 1) & 0xFF;
  this.setFlag(Flags6502.B, false);

  const lo = this.read(0xFFFE);
  const hi = this.read(0xFFFF);
  this.pc = (hi << 8) | lo;
  return 0;
}
```
:::
