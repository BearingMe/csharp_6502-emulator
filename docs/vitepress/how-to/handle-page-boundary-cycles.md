---
title: How to Emulate 6502 Page Boundary Penalties
description: Detailed rules and implementation logic for cycle penalties on memory page crossings and conditional branches.
---

On real 6502 hardware, 16-bit address arithmetic is split into two clock phases: low-byte addition occurs in cycle 1, and high-byte carry correction occurs in cycle 2 only if an overflow from the low byte took place.

## When Page Crossing Penalties Apply

Page boundary penalties add **+1 clock cycle** under specific conditions:

| Operation Category | Addressing Mode | Condition for +1 Cycle |
| :--- | :--- | :--- |
| **Read Instructions** (`LDA`, `LDX`, `LDY`, `ADC`, `SBC`, `AND`, `ORA`, `EOR`, `CMP`, etc.) | `ABX`, `ABY`, `IZY` | When $((\text{base} + \text{index}) \land 0\text{xFF00}) \neq (\text{base} \land 0\text{xFF00})$ |
| **Store Instructions** (`STA`, `STX`, `STY`) | `ABX`, `ABY`, `IZY` | **Never** adds dynamic cycle (always fixed 5 or 6 cycles because writes cannot skip the dummy read) |
| **Read-Modify-Write Instructions** (`ASL`, `LSR`, `ROL`, `ROR`, `INC`, `DEC`) | `ABX` | **Never** dynamic (always fixed 7 cycles) |
| **Conditional Branches** (`BCC`, `BCS`, `BEQ`, `BNE`, `BMI`, `BPL`, `BVC`, `BVS`) | `REL` | +1 if branch taken, **+1 additional** if target crosses into a new page |

---

## 1. Implementing Page Cross Detection in Addressing Modes

Have addressing modes return `1` if a page cross occurred:

::: code-group
```cpp [C++]
uint8_t CPU6502::ABX() {
    uint16_t lo = read(pc++);
    uint16_t hi = read(pc++);
    
    addr_abs = ((hi << 8) | lo) + x;

    // Check if the high byte changed
    if ((addr_abs & 0xFF00) != (hi << 8)) {
        return 1; // Potential extra cycle
    }
    return 0;
}
```

```rust [Rust]
pub fn abx(&mut self) -> u8 {
    let lo = self.read(self.pc) as u16;
    self.pc += 1;
    let hi = self.read(self.pc) as u16;
    self.pc += 1;

    self.addr_abs = ((hi << 8) | lo).wrapping_add(self.x as u16);

    // Check if the high byte changed
    if (self.addr_abs & 0xFF00) != (hi << 8) {
        1 // Potential extra cycle
    } else {
        0
    }
}
```

```typescript [TypeScript]
public ABX(): number {
  const lo = this.read(this.pc++);
  const hi = this.read(this.pc++);

  this.addrAbs = (((hi << 8) | lo) + this.x) & 0xFFFF;

  // Check if the high byte changed
  if ((this.addrAbs & 0xFF00) !== (hi << 8)) {
    return 1; // Potential extra cycle
  }
  return 0;
}
```
:::

---

## 2. Implementing Page Cross Detection in Operations

Have read instructions return `1` to indicate they can benefit from page-crossing cycle additions:

::: code-group
```cpp [C++]
uint8_t CPU6502::LDA() {
    fetch();
    a = fetched;
    updateNZ(a);
    return 1; // Can consume extra cycle
}

uint8_t CPU6502::STA() {
    write(addr_abs, a);
    return 0; // Store operations do NOT add a page cross cycle
}
```

```rust [Rust]
pub fn lda(&mut self) -> u8 {
    self.fetch();
    self.a = self.fetched;
    self.update_nz(self.a);
    1 // Can consume extra cycle
}

pub fn sta(&mut self) -> u8 {
    self.write(self.addr_abs, self.a);
    0 // Store operations do NOT add a page cross cycle
}
```

```typescript [TypeScript]
public LDA(): number {
  this.fetch();
  this.a = this.fetched;
  this.updateNZ(this.a);
  return 1; // Can consume extra cycle
}

public STA(): number {
  this.write(this.addrAbs, this.a);
  return 0; // Store operations do NOT add a page cross cycle
}
```
:::

In the CPU clock stepper:

::: code-group
```cpp [C++]
uint8_t extra_addr = (this->*lookup[opcode].addrmode)();
uint8_t extra_op   = (this->*lookup[opcode].operate)();

// Only add cycle if BOTH agree
cycles += (extra_addr & extra_op);
```

```rust [Rust]
let extra_addr = (inst.addrmode)(self);
let extra_op = (inst.operate)(self);

// Only add cycle if BOTH agree
self.cycles += extra_addr & extra_op;
```

```typescript [TypeScript]
const extraAddr = inst.addrmode();
const extraOp = inst.operate();

// Only add cycle if BOTH agree
this.cycles += (extraAddr & extraOp);
```
:::

---

## 3. Implementing Branch Page Cross Logic

Branch instructions evaluate the condition, add 1 cycle if taken, and add another cycle if the target crosses a page:

::: code-group
```cpp [C++]
uint8_t CPU6502::branchIf(bool condition) {
    if (condition) {
        cycles++; // +1 cycle for taking the branch
        
        addr_abs = pc + addr_rel;

        // Check if branch target is on a different page than the PC
        if ((addr_abs & 0xFF00) != (pc & 0xFF00)) {
            cycles++; // +1 extra cycle for page boundary crossing
        }

        pc = addr_abs;
    }
    return 0;
}

uint8_t CPU6502::BEQ() { return branchIf(getFlag(Flags6502::Z) == 1); }
uint8_t CPU6502::BNE() { return branchIf(getFlag(Flags6502::Z) == 0); }
uint8_t CPU6502::BCS() { return branchIf(getFlag(Flags6502::C) == 1); }
uint8_t CPU6502::BCC() { return branchIf(getFlag(Flags6502::C) == 0); }
```

```rust [Rust]
pub fn branch_if(&mut self, condition: bool) -> u8 {
    if condition {
        self.cycles += 1; // +1 cycle for taking the branch

        self.addr_abs = self.pc.wrapping_add(self.addr_rel);

        // Check if branch target is on a different page than the PC
        if (self.addr_abs & 0xFF00) != (self.pc & 0xFF00) {
            self.cycles += 1; // +1 extra cycle for page boundary crossing
        }

        self.pc = self.addr_abs;
    }
    0
}

pub fn beq(&mut self) -> u8 { let z = self.get_flag(Flag::Z); self.branch_if(z) }
pub fn bne(&mut self) -> u8 { let z = self.get_flag(Flag::Z); self.branch_if(!z) }
pub fn bcs(&mut self) -> u8 { let c = self.get_flag(Flag::C); self.branch_if(c) }
pub fn bcc(&mut self) -> u8 { let c = self.get_flag(Flag::C); self.branch_if(!c) }
```

```typescript [TypeScript]
private branchIf(condition: boolean): number {
  if (condition) {
    this.cycles++; // +1 cycle for taking the branch

    this.addrAbs = (this.pc + this.addrRel) & 0xFFFF;

    // Check if branch target is on a different page than the PC
    if ((this.addrAbs & 0xFF00) !== (this.pc & 0xFF00)) {
      this.cycles++; // +1 extra cycle for page boundary crossing
    }

    this.pc = this.addrAbs;
  }
  return 0;
}

public BEQ(): number { return this.branchIf(this.getFlag(Flags6502.Z) === 1); }
public BNE(): number { return this.branchIf(this.getFlag(Flags6502.Z) === 0); }
public BCS(): number { return this.branchIf(this.getFlag(Flags6502.C) === 1); }
public BCC(): number { return this.branchIf(this.getFlag(Flags6502.C) === 0); }
```
:::
