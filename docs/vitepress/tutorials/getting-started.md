---
title: 1. Architecture & Memory Bus
description: Scaffolding the 6502 modular structure and connecting it to a generic 16-bit address memory bus.
---

The MOS 6502 interacts with the external world strictly through an 8-bit data bus and a 16-bit address bus. To maintain clean architectural boundaries and testability, your emulator must decouple the CPU core from memory devices through a Bus abstraction.

## The 64KB Address Space

The 6502 can access $0000 to $FFFF ($2^{16} = 65,536$ address locations). The memory map is divided into 256 "pages" of 256 bytes each:

| Address Range | Page | Description |
| :--- | :--- | :--- |
| `$0000 - $00FF` | Page 0 | **Zero Page**: Fast access 1-byte addressed memory |
| `$0100 - $01FF` | Page 1 | **Processor Stack**: 256-byte LIFO stack (descending) |
| `$0200 - $07FF` | Page 2-7 | General Purpose RAM |
| `$0800 - $DFFF` | Pages 8-223 | System RAM / ROM / Memory Mapped I/O |
| `$FFFA - $FFFF` | Page 255 | **Hardware Vectors** (NMI: `$FFFA`, Reset: `$FFFC`, IRQ/BRK: `$FFFE`) |

## Step 1: Implementing the Bus Interface

Create a `Bus` abstraction that models memory read and write operations.

::: code-group
```cpp [C++]
#pragma once
#include <cstdint>
#include <array>

class Bus {
public:
    Bus() {
        // Clear RAM
        ram.fill(0x00);
    }

    uint8_t read(uint16_t addr, bool readOnly = false) {
        if (addr >= 0x0000 && addr <= 0xFFFF) {
            return ram[addr];
        }
        return 0x00;
    }

    void write(uint16_t addr, uint8_t data) {
        if (addr >= 0x0000 && addr <= 0xFFFF) {
            ram[addr] = data;
        }
    }

public:
    std::array<uint8_t, 64 * 1024> ram;
};
```

```rust [Rust]
pub struct Bus {
    pub ram: [u8; 65536],
}

impl Bus {
    pub fn new() -> Self {
        Self { ram: [0; 65536] }
    }

    pub fn read(&self, addr: u16) -> u8 {
        self.ram[addr as usize]
    }

    pub fn write(&mut self, addr: u16, data: u8) {
        self.ram[addr as usize] = data;
    }
}
```

```typescript [TypeScript]
export class Bus {
  public ram = new Uint8Array(65536);

  public read(addr: number, _readOnly = false): number {
    return this.ram[addr & 0xFFFF];
  }

  public write(addr: number, data: number): void {
    this.ram[addr & 0xFFFF] = data & 0xFF;
  }
}
```
:::

## Step 2: CPU Core Skeleton

Create the basic CPU structure that holds a reference to the bus and maintains internal state:

::: code-group
```cpp [C++]
class CPU6502 {
public:
    CPU6502() = default;
    void connectBus(Bus* b) { bus = b; }

    uint8_t read(uint16_t addr) {
        return bus->read(addr, false);
    }

    void write(uint16_t addr, uint8_t data) {
        bus->write(addr, data);
    }

private:
    Bus* bus = nullptr;
};
```

```rust [Rust]
pub struct CPU6502<'a> {
    pub bus: &'a mut Bus,
}

impl<'a> CPU6502<'a> {
    pub fn new(bus: &'a mut Bus) -> Self {
        Self { bus }
    }

    pub fn read(&self, addr: u16) -> u8 {
        self.bus.read(addr)
    }

    pub fn write(&mut self, addr: u16, data: u8) {
        self.bus.write(addr, data);
    }
}
```

```typescript [TypeScript]
import { Bus } from './Bus';

export class CPU6502 {
  private bus: Bus | null = null;

  public connectBus(b: Bus): void {
    this.bus = b;
  }

  public read(addr: number): number {
    return this.bus ? this.bus.read(addr, false) : 0x00;
  }

  public write(addr: number, data: number): void {
    if (this.bus) {
      this.bus.write(addr, data);
    }
  }
}
```
:::

## Verification Checklist

1. Can the CPU read from address `$0000` through `$FFFF` without crashing?
2. Does writing `$42` to `$0200` return `$42` when reading from `$0200`?
3. Does writing outside the address range wrap or truncate cleanly to 16 bits?
