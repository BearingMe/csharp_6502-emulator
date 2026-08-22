---
title: How to Build a Step-by-Step CPU Disassembler
description: Creating a memory range disassembler that transforms raw 6502 machine bytes into standard mnemonic assembly.
---

A disassembler is an essential debugging component for any 6502 emulator. It iterates over a memory buffer, identifies opcodes, determines operand lengths from addressing modes, and formats human-readable assembly lines.

## Disassembler Output Example

```text
$C000: A9 01     LDA #$01       {IMM}
$C002: 8D 00 20  STA $2000      {ABS}
$C005: D0 FB     BNE $C002      {REL}
```

---

## Disassembly Implementation Recipe

::: code-group
```cpp [C++]
#include <map>
#include <string>
#include <iomanip>
#include <sstream>

std::map<uint16_t, std::string> CPU6502::disassemble(uint16_t nStart, uint16_t nStop) {
    uint32_t addr = nStart;
    std::map<uint16_t, std::string> mapLines;

    auto hexStr = [](uint32_t n, uint8_t d) {
        std::string s(d, '0');
        for (int i = d - 1; i >= 0; i--, n >>= 4)
            s[i] = "0123456789ABCDEF"[n & 0xF];
        return s;
    };

    while (addr <= (uint32_t)nStop) {
        uint16_t line_addr = (uint16_t)addr;
        std::string sInst = "$" + hexStr(addr, 4) + ": ";

        // Read opcode byte
        uint8_t op = bus->read(addr++, true);
        sInst += lookup[op].name + " ";

        // Format operand based on addressing mode
        if (lookup[op].addrmode == &CPU6502::IMP) {
            sInst += "{IMP}";
        }
        else if (lookup[op].addrmode == &CPU6502::IMM) {
            uint8_t val = bus->read(addr++, true);
            sInst += "#$" + hexStr(val, 2) + " {IMM}";
        }
        else if (lookup[op].addrmode == &CPU6502::ZP0) {
            uint8_t lo = bus->read(addr++, true);
            sInst += "$" + hexStr(lo, 2) + " {ZP0}";
        }
        else if (lookup[op].addrmode == &CPU6502::ZPX) {
            uint8_t lo = bus->read(addr++, true);
            sInst += "$" + hexStr(lo, 2) + ", X {ZPX}";
        }
        else if (lookup[op].addrmode == &CPU6502::ZPY) {
            uint8_t lo = bus->read(addr++, true);
            sInst += "$" + hexStr(lo, 2) + ", Y {ZPY}";
        }
        else if (lookup[op].addrmode == &CPU6502::ABS) {
            uint8_t lo = bus->read(addr++, true);
            uint8_t hi = bus->read(addr++, true);
            sInst += "$" + hexStr((hi << 8) | lo, 4) + " {ABS}";
        }
        else if (lookup[op].addrmode == &CPU6502::ABX) {
            uint8_t lo = bus->read(addr++, true);
            uint8_t hi = bus->read(addr++, true);
            sInst += "$" + hexStr((hi << 8) | lo, 4) + ", X {ABX}";
        }
        else if (lookup[op].addrmode == &CPU6502::ABY) {
            uint8_t lo = bus->read(addr++, true);
            uint8_t hi = bus->read(addr++, true);
            sInst += "$" + hexStr((hi << 8) | lo, 4) + ", Y {ABY}";
        }
        else if (lookup[op].addrmode == &CPU6502::IND) {
            uint8_t lo = bus->read(addr++, true);
            uint8_t hi = bus->read(addr++, true);
            sInst += "($" + hexStr((hi << 8) | lo, 4) + ") {IND}";
        }
        else if (lookup[op].addrmode == &CPU6502::IZX) {
            uint8_t lo = bus->read(addr++, true);
            sInst += "($" + hexStr(lo, 2) + ", X) {IZX}";
        }
        else if (lookup[op].addrmode == &CPU6502::IZY) {
            uint8_t lo = bus->read(addr++, true);
            sInst += "($" + hexStr(lo, 2) + "), Y {IZY}";
        }
        else if (lookup[op].addrmode == &CPU6502::REL) {
            uint8_t offset = bus->read(addr++, true);
            uint16_t target = addr + (int8_t)offset;
            sInst += "$" + hexStr(offset, 2) + " [$" + hexStr(target, 4) + "] {REL}";
        }

        mapLines[line_addr] = sInst;
    }

    return mapLines;
}
```

```rust [Rust]
use std::collections::BTreeMap;

impl<'a> CPU6502<'a> {
    pub fn disassemble(&self, n_start: u16, n_stop: u16) -> BTreeMap<u16, String> {
        let mut map_lines = BTreeMap::new();
        let mut addr = n_start as u32;

        while addr <= (n_stop as u32) {
            let line_addr = addr as u16;
            let mut s_inst = format!("${:04X}: ", line_addr);

            let op = self.bus.read(addr as u16);
            addr += 1;

            let inst = &self.lookup[op as usize];
            s_inst.push_str(inst.name);
            s_inst.push(' ');

            match inst.addrmode_tag {
                AddrModeTag::IMP => {
                    s_inst.push_str("{IMP}");
                }
                AddrModeTag::IMM => {
                    let val = self.bus.read(addr as u16);
                    addr += 1;
                    s_inst.push_str(&format!("#${:02X} {{IMM}}", val));
                }
                AddrModeTag::ZP0 => {
                    let lo = self.bus.read(addr as u16);
                    addr += 1;
                    s_inst.push_str(&format!("${:02X} {{ZP0}}", lo));
                }
                AddrModeTag::ZPX => {
                    let lo = self.bus.read(addr as u16);
                    addr += 1;
                    s_inst.push_str(&format!("${:02X}, X {{ZPX}}", lo));
                }
                AddrModeTag::ZPY => {
                    let lo = self.bus.read(addr as u16);
                    addr += 1;
                    s_inst.push_str(&format!("${:02X}, Y {{ZPY}}", lo));
                }
                AddrModeTag::ABS => {
                    let lo = self.bus.read(addr as u16) as u16;
                    addr += 1;
                    let hi = self.bus.read(addr as u16) as u16;
                    addr += 1;
                    s_inst.push_str(&format!("${:04X} {{ABS}}", (hi << 8) | lo));
                }
                AddrModeTag::ABX => {
                    let lo = self.bus.read(addr as u16) as u16;
                    addr += 1;
                    let hi = self.bus.read(addr as u16) as u16;
                    addr += 1;
                    s_inst.push_str(&format!("${:04X}, X {{ABX}}", (hi << 8) | lo));
                }
                AddrModeTag::ABY => {
                    let lo = self.bus.read(addr as u16) as u16;
                    addr += 1;
                    let hi = self.bus.read(addr as u16) as u16;
                    addr += 1;
                    s_inst.push_str(&format!("${:04X}, Y {{ABY}}", (hi << 8) | lo));
                }
                AddrModeTag::IND => {
                    let lo = self.bus.read(addr as u16) as u16;
                    addr += 1;
                    let hi = self.bus.read(addr as u16) as u16;
                    addr += 1;
                    s_inst.push_str(&format!("(${:04X}) {{IND}}", (hi << 8) | lo));
                }
                AddrModeTag::IZX => {
                    let lo = self.bus.read(addr as u16);
                    addr += 1;
                    s_inst.push_str(&format!("(${:02X}, X) {{IZX}}", lo));
                }
                AddrModeTag::IZY => {
                    let lo = self.bus.read(addr as u16);
                    addr += 1;
                    s_inst.push_str(&format!("(${:02X}), Y {{IZY}}", lo));
                }
                AddrModeTag::REL => {
                    let offset = self.bus.read(addr as u16);
                    addr += 1;
                    let target = (addr as i32 + (offset as i8 as i32)) as u16;
                    s_inst.push_str(&format!("${:02X} [${:04X}] {{REL}}", offset, target));
                }
            }

            map_lines.insert(line_addr, s_inst);
        }

        map_lines
    }
}
```

```typescript [TypeScript]
export class CPU6502 {
  public disassemble(nStart: number, nStop: number): Map<number, string> {
    let addr = nStart;
    const mapLines = new Map<number, string>();

    const hexStr = (n: number, d: number) => n.toString(16).toUpperCase().padStart(d, '0');

    while (addr <= nStop) {
      const lineAddr = addr;
      let sInst = `$${hexStr(addr, 4)}: `;

      const op = this.bus ? this.bus.read(addr++, true) : 0;
      const inst = this.lookup[op];
      sInst += `${inst.name} `;

      if (inst.addrmode === this.IMP) {
        sInst += '{IMP}';
      } else if (inst.addrmode === this.IMM) {
        const val = this.bus ? this.bus.read(addr++, true) : 0;
        sInst += `#$${hexStr(val, 2)} {IMM}`;
      } else if (inst.addrmode === this.ZP0) {
        const lo = this.bus ? this.bus.read(addr++, true) : 0;
        sInst += `$${hexStr(lo, 2)} {ZP0}`;
      } else if (inst.addrmode === this.ZPX) {
        const lo = this.bus ? this.bus.read(addr++, true) : 0;
        sInst += `$${hexStr(lo, 2)}, X {ZPX}`;
      } else if (inst.addrmode === this.ZPY) {
        const lo = this.bus ? this.bus.read(addr++, true) : 0;
        sInst += `$${hexStr(lo, 2)}, Y {ZPY}`;
      } else if (inst.addrmode === this.ABS) {
        const lo = this.bus ? this.bus.read(addr++, true) : 0;
        const hi = this.bus ? this.bus.read(addr++, true) : 0;
        sInst += `$${hexStr((hi << 8) | lo, 4)} {ABS}`;
      } else if (inst.addrmode === this.ABX) {
        const lo = this.bus ? this.bus.read(addr++, true) : 0;
        const hi = this.bus ? this.bus.read(addr++, true) : 0;
        sInst += `$${hexStr((hi << 8) | lo, 4)}, X {ABX}`;
      } else if (inst.addrmode === this.ABY) {
        const lo = this.bus ? this.bus.read(addr++, true) : 0;
        const hi = this.bus ? this.bus.read(addr++, true) : 0;
        sInst += `$${hexStr((hi << 8) | lo, 4)}, Y {ABY}`;
      } else if (inst.addrmode === this.IND) {
        const lo = this.bus ? this.bus.read(addr++, true) : 0;
        const hi = this.bus ? this.bus.read(addr++, true) : 0;
        sInst += `($${hexStr((hi << 8) | lo, 4)}) {IND}`;
      } else if (inst.addrmode === this.IZX) {
        const lo = this.bus ? this.bus.read(addr++, true) : 0;
        sInst += `($${hexStr(lo, 2)}, X) {IZX}`;
      } else if (inst.addrmode === this.IZY) {
        const lo = this.bus ? this.bus.read(addr++, true) : 0;
        sInst += `($${hexStr(lo, 2)}), Y {IZY}`;
      } else if (inst.addrmode === this.REL) {
        const offset = this.bus ? this.bus.read(addr++, true) : 0;
        const signedOffset = (offset & 0x80) ? (offset - 256) : offset;
        const target = (addr + signedOffset) & 0xFFFF;
        sInst += `$${hexStr(offset, 2)} [$${hexStr(target, 4)}] {REL}`;
      }

      mapLines.set(lineAddr, sInst);
    }

    return mapLines;
  }
}
```
:::
