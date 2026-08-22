---
title: Cycle-Accurate vs Step-Level Emulation
description: Comparing instruction-level cycle countdown timers with true cycle-by-cycle micro-operation state machines.
---

When designing a 6502 emulator, you must choose between two major architectural strategies: **Instruction-Level Step Execution** and **Cycle-Stepping State Machines**.

## 1. Instruction-Level Stepping (Atomic Execution)

In this design, when an instruction begins, the emulator computes the complete state change (address resolution, memory reads/writes, ALU math, flag updates) in a single synchronous pass. The total number of required cycles is calculated and loaded into a cycle countdown timer.

::: code-group
```cpp [C++]
void CPU6502::clock() {
    if (cycles == 0) {
        opcode = read(pc++);
        cycles = lookup[opcode].cycles;

        uint8_t addrmode_extra = (this->*lookup[opcode].addrmode)();
        uint8_t opcode_extra   = (this->*lookup[opcode].operate)();

        cycles += (addrmode_extra & opcode_extra);
    }
    cycles--;
}
```

```rust [Rust]
pub fn clock(&mut self) {
    if self.cycles == 0 {
        self.opcode = self.read(self.pc);
        self.pc += 1;

        let inst = &self.lookup[self.opcode as usize];
        self.cycles = inst.cycles;

        let addrmode_extra = (inst.addrmode)(self);
        let opcode_extra = (inst.operate)(self);

        self.cycles += addrmode_extra & opcode_extra;
    }
    self.cycles = self.cycles.saturating_sub(1);
}
```

```typescript [TypeScript]
public clock(): void {
  if (this.cycles === 0) {
    this.opcode = this.read(this.pc++);
    const inst = this.lookup[this.opcode];
    this.cycles = inst.cycles;

    const addrmodeExtra = inst.addrmode();
    const opcodeExtra = inst.operate();

    this.cycles += (addrmodeExtra & opcodeExtra);
  }
  this.cycles--;
}
```
:::

### Advantages
- **Simplicity**: Easy to read, write, and debug. Each opcode is a self-contained function.
- **High Performance**: Minimal function call overhead and branch prediction pressure.
- **Sufficient for Most Emulators**: Runs 98%+ of Apple II, Commodore 64, and NES software accurately when integrated with standard frame/scanline synchronizers.

### Limitations
- Cannot interleave mid-instruction hardware bus reads/writes with other chips (e.g. mid-instruction PPU raster line changes).

---

## 2. Cycle-by-Cycle Micro-Operation State Machine

In this approach, the emulator models each individual clock cycle (e.g. Cycle 1: Fetch Opcode, Cycle 2: Fetch low address, Cycle 3: Fetch high address, Cycle 4: Read memory byte).

### When True Cycle-Stepping Is Necessary
- Emulating precise mid-instruction bus timing (e.g. NES mapper chips that snoop every single CPU cycle address pin).
- Emulating open-bus quirks and intermediate dummy reads/writes on read-modify-write instructions (`ASL`, `INC`).
