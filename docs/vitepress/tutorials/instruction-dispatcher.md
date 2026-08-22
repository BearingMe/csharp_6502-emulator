---
title: 5. Instruction Dispatcher & Stepper
description: Connecting addressing modes and ALU operations into a 256-entry opcode matrix and clock stepper.
---

With the bus, registers, addressing mode resolvers, and ALU instructions built, the next step is wiring them into an instruction decoder and execution clock stepper.

## Opcode Table Architecture

Each 6502 instruction maps an 8-bit opcode (`$00` to `$FF`) to:
1. **Mnemonic string** (e.g., `"LDA"`, `"ADC"`)
2. **Operation function pointer** (e.g., `&CPU6502::LDA`)
3. **Addressing mode function pointer** (e.g., `&CPU6502::ABX`)
4. **Base cycle count** (e.g., `4`)

::: code-group
```cpp [C++]
struct Instruction {
    std::string name;
    uint8_t (CPU6502::*operate)(void)  = nullptr;
    uint8_t (CPU6502::*addrmode)(void) = nullptr;
    uint8_t cycles = 0;
};

std::vector<Instruction> lookup;
```

```rust [Rust]
pub struct Instruction<'a> {
    pub name: &'static str,
    pub operate: fn(&mut CPU6502) -> u8,
    pub addrmode: fn(&mut CPU6502) -> u8,
    pub cycles: u8,
}

pub type LookupTable<'a> = [Instruction<'a>; 256];
```

```typescript [TypeScript]
export interface Instruction {
  name: string;
  operate: () => number;
  addrmode: () => number;
  cycles: number;
}

export type LookupTable = Instruction[];
```
:::

## Step 1: Populating the 16x16 Opcode Table

The opcode byte is indexed with the upper nibble as row and lower nibble as column:

::: code-group
```cpp [C++]
void CPU6502::initLookup() {
    using a = CPU6502;
    lookup = {
        { "BRK", &a::BRK, &a::IMM, 7 }, { "ORA", &a::ORA, &a::IZX, 6 }, { "???", &a::NOP, &a::IMP, 2 }, { "???", &a::NOP, &a::IMP, 8 },
        { "???", &a::NOP, &a::IMP, 3 }, { "ORA", &a::ORA, &a::ZP0, 3 }, { "ASL", &a::ASL, &a::ZP0, 5 }, { "???", &a::NOP, &a::IMP, 5 },
        { "PHP", &a::PHP, &a::IMP, 3 }, { "ORA", &a::ORA, &a::IMM, 2 }, { "ASL", &a::ASL, &a::IMP, 2 }, { "???", &a::NOP, &a::IMP, 2 },
        { "???", &a::NOP, &a::IMP, 4 }, { "ORA", &a::ORA, &a::ABS, 4 }, { "ASL", &a::ASL, &a::ABS, 6 }, { "???", &a::NOP, &a::IMP, 6 },
        // ... (remaining 240 opcode entries)
    };
}
```

```rust [Rust]
pub fn create_lookup_table<'a>() -> [Instruction<'a>; 256] {
    [
        Instruction { name: "BRK", operate: CPU6502::brk, addrmode: CPU6502::imm, cycles: 7 },
        Instruction { name: "ORA", operate: CPU6502::ora, addrmode: CPU6502::izx, cycles: 6 },
        Instruction { name: "???", operate: CPU6502::nop, addrmode: CPU6502::imp, cycles: 2 },
        Instruction { name: "???", operate: CPU6502::nop, addrmode: CPU6502::imp, cycles: 8 },
        Instruction { name: "???", operate: CPU6502::nop, addrmode: CPU6502::imp, cycles: 3 },
        Instruction { name: "ORA", operate: CPU6502::ora, addrmode: CPU6502::zp0, cycles: 3 },
        Instruction { name: "ASL", operate: CPU6502::asl, addrmode: CPU6502::zp0, cycles: 5 },
        Instruction { name: "???", operate: CPU6502::nop, addrmode: CPU6502::imp, cycles: 5 },
        Instruction { name: "PHP", operate: CPU6502::php, addrmode: CPU6502::imp, cycles: 3 },
        Instruction { name: "ORA", operate: CPU6502::ora, addrmode: CPU6502::imm, cycles: 2 },
        Instruction { name: "ASL", operate: CPU6502::asl, addrmode: CPU6502::imp, cycles: 2 },
        Instruction { name: "???", operate: CPU6502::nop, addrmode: CPU6502::imp, cycles: 2 },
        Instruction { name: "???", operate: CPU6502::nop, addrmode: CPU6502::imp, cycles: 4 },
        Instruction { name: "ORA", operate: CPU6502::ora, addrmode: CPU6502::abs, cycles: 4 },
        Instruction { name: "ASL", operate: CPU6502::asl, addrmode: CPU6502::abs, cycles: 6 },
        Instruction { name: "???", operate: CPU6502::nop, addrmode: CPU6502::imp, cycles: 6 },
        // ... (remaining 240 opcode entries)
    ]
}
```

```typescript [TypeScript]
public initLookup(): void {
  this.lookup = [
    { name: "BRK", operate: this.BRK.bind(this), addrmode: this.IMM.bind(this), cycles: 7 },
    { name: "ORA", operate: this.ORA.bind(this), addrmode: this.IZX.bind(this), cycles: 6 },
    { name: "???", operate: this.NOP.bind(this), addrmode: this.IMP.bind(this), cycles: 2 },
    { name: "???", operate: this.NOP.bind(this), addrmode: this.IMP.bind(this), cycles: 8 },
    { name: "???", operate: this.NOP.bind(this), addrmode: this.IMP.bind(this), cycles: 3 },
    { name: "ORA", operate: this.ORA.bind(this), addrmode: this.ZP0.bind(this), cycles: 3 },
    { name: "ASL", operate: this.ASL.bind(this), addrmode: this.ZP0.bind(this), cycles: 5 },
    { name: "???", operate: this.NOP.bind(this), addrmode: this.IMP.bind(this), cycles: 5 },
    { name: "PHP", operate: this.PHP.bind(this), addrmode: this.IMP.bind(this), cycles: 3 },
    { name: "ORA", operate: this.ORA.bind(this), addrmode: this.IMM.bind(this), cycles: 2 },
    { name: "ASL", operate: this.ASL.bind(this), addrmode: this.IMP.bind(this), cycles: 2 },
    { name: "???", operate: this.NOP.bind(this), addrmode: this.IMP.bind(this), cycles: 2 },
    { name: "???", operate: this.NOP.bind(this), addrmode: this.IMP.bind(this), cycles: 4 },
    { name: "ORA", operate: this.ORA.bind(this), addrmode: this.ABS.bind(this), cycles: 4 },
    { name: "ASL", operate: this.ASL.bind(this), addrmode: this.ABS.bind(this), cycles: 6 },
    { name: "???", operate: this.NOP.bind(this), addrmode: this.IMP.bind(this), cycles: 6 },
    // ... (remaining 240 opcode entries)
  ];
}
```
:::

## Step 2: Cycle-Based Clock Stepping

When `clock()` is invoked:
1. If `cycles == 0`, fetch next opcode from memory at `PC`, increment `PC`.
2. Retrieve base cycle count for the opcode.
3. Call the addressing mode function to prepare addresses and compute potential page boundary cycles (`extra_cycle1`).
4. Call the operation function (`extra_cycle2`).
5. Add extra cycle only if **both** the addressing mode and the instruction permit page cross extensions (`cycles += (extra_cycle1 & extra_cycle2)`).
6. Decrement `cycles`.

::: code-group
```cpp [C++]
void CPU6502::clock() {
    if (cycles == 0) {
        // Read opcode byte
        opcode = read(pc);
        
        // Unused flag U is always 1
        setFlag(Flags6502::U, true);
        
        pc++;

        // Base cycles
        cycles = lookup[opcode].cycles;

        // Addressing resolution
        uint8_t addrmode_extra = (this->*lookup[opcode].addrmode)();

        // Instruction execution
        uint8_t opcode_extra = (this->*lookup[opcode].operate)();

        // Add cycle if both conditions satisfied
        cycles += (addrmode_extra & opcode_extra);

        setFlag(Flags6502::U, true);
    }

    cycles--;
}
```

```rust [Rust]
pub fn clock(&mut self) {
    if self.cycles == 0 {
        self.opcode = self.read(self.pc);
        self.set_flag(Flag::U, true);
        self.pc += 1;

        let inst = &self.lookup[self.opcode as usize];
        self.cycles = inst.cycles;

        let addrmode_fn = inst.addrmode;
        let operate_fn = inst.operate;

        let addrmode_extra = addrmode_fn(self);
        let opcode_extra = operate_fn(self);

        self.cycles += addrmode_extra & opcode_extra;
        self.set_flag(Flag::U, true);
    }

    self.cycles = self.cycles.saturating_sub(1);
}
```

```typescript [TypeScript]
public clock(): void {
  if (this.cycles === 0) {
    this.opcode = this.read(this.pc);
    this.setFlag(Flags6502.U, true);
    this.pc++;

    const inst = this.lookup[this.opcode];
    this.cycles = inst.cycles;

    const addrmodeExtra = inst.addrmode();
    const opcodeExtra = inst.operate();

    this.cycles += (addrmodeExtra & opcodeExtra);
    this.setFlag(Flags6502.U, true);
  }

  this.cycles--;
}
```
:::

## Step 3: Instruction Stepping Helper

To step an entire instruction at a time (e.g. for debugging or unit testing):

::: code-group
```cpp [C++]
void CPU6502::step() {
    do {
        clock();
    } while (cycles > 0);
}
```

```rust [Rust]
pub fn step(&mut self) {
    loop {
        self.clock();
        if self.cycles == 0 {
            break;
        }
    }
}
```

```typescript [TypeScript]
public step(): void {
  do {
    this.clock();
  } while (this.cycles > 0);
}
```
:::
