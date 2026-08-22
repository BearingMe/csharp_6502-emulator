---
title: How to Replicate the Indirect JMP Hardware Bug
description: Accurately emulating the famous NMOS 6502 page-wrap bug on JMP ($xxFF).
---

The original NMOS 6502 microprocessor has an erratum in the implementation of the `JMP ($xxxx)` (Indirect Jump) instruction when the pointer lies across a page boundary (`$xxFF`).

## The Hardware Erratum

When executing `JMP ($10FF)`:
- **Expected Behavior**: The CPU reads the low byte from `$10FF` and the high byte from `$1100`.
- **Actual NMOS Behavior**: The CPU reads the low byte from `$10FF`, but when incrementing the pointer address to fetch the high byte, the hardware fails to carry into the page high byte. It instead reads the high byte from `$1000`!

::: warning Compatibility Impact
Many vintage games and software (e.g. Commodore 64 and NES titles) rely on exact cycle counts and avoid `$xxFF` pointers, but comprehensive test suites (like Bruce Clark's decimal tests or Kevtris's nestest) verify this bug explicitly.
:::

---

## Implementation Recipe

When decoding `JMP ($xxxx)` (mode `IND`), check if `ptr_lo == 0x00FF`:

::: code-group
```cpp [C++]
uint8_t CPU6502::IND() {
    uint16_t ptr_lo = read(pc++);
    uint16_t ptr_hi = read(pc++);

    uint16_t ptr = (ptr_hi << 8) | ptr_lo;

    if (ptr_lo == 0x00FF) {
        // Buggy behavior: high byte wraps back to start of current page ($xx00)
        uint16_t lo = read(ptr);
        uint16_t hi = read(ptr & 0xFF00);
        addr_abs = (hi << 8) | lo;
    } else {
        // Normal behavior: high byte is read from ptr + 1
        uint16_t lo = read(ptr);
        uint16_t hi = read(ptr + 1);
        addr_abs = (hi << 8) | lo;
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
        // Buggy behavior: high byte wraps back to start of current page ($xx00)
        let lo = self.read(ptr) as u16;
        let hi = self.read(ptr & 0xFF00) as u16;
        self.addr_abs = (hi << 8) | lo;
    } else {
        // Normal behavior: high byte is read from ptr + 1
        let lo = self.read(ptr) as u16;
        let hi = self.read(ptr + 1) as u16;
        self.addr_abs = (hi << 8) | lo;
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
    // Buggy behavior: high byte wraps back to start of current page ($xx00)
    const lo = this.read(ptr);
    const hi = this.read(ptr & 0xFF00);
    this.addrAbs = (hi << 8) | lo;
  } else {
    // Normal behavior: high byte is read from ptr + 1
    const lo = this.read(ptr);
    const hi = this.read(ptr + 1);
    this.addrAbs = (hi << 8) | lo;
  }

  return 0;
}
```
:::

## CMOS 65C02 Difference

In the later CMOS 65C02 microprocessor by Western Design Center (WDC), this bug was fixed to correctly read from `$1100`, requiring one additional clock cycle. If you are specifically targeting NMOS 6502 (NES, Apple II, Commodore 64), keep the buggy wrap enabled.
