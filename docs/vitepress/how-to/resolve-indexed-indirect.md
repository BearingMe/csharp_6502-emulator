---
title: How to Resolve Indexed Indirect Addressing Modes (IZX & IZY)
description: Complete step-by-step resolution logic and zero-page wrap mechanics for ($LL, X) and ($LL), Y addressing modes.
---

Indexed indirect modes provide pointer-based memory access on the 6502. Because the two modes differ subtly in when indexing is applied (before vs. after pointer dereferencing), they are commonly confused.

## Quick Distinction

| Notation | Name | Index Register | When Index Is Added | Wraparound Behavior |
| :--- | :--- | :---: | :--- | :--- |
| `($LL, X)` | **Indexed Indirect** (IZX) | `X` | **Before** pointer dereference | Always wraps in Zero Page (`$00..$FF`) |
| `($LL), Y` | **Indirect Indexed** (IZY) | `Y` | **After** pointer dereference | 16-bit addition (may cross page boundary) |

---

## 1. Resolving `($LL, X)` (Pre-Indexed Indirect)

### Step-by-Step Execution
1. Fetch 8-bit base address $LL$ from instruction stream.
2. Add the `X` register to $LL$ modulo 256: $\text{ptr} = (LL + X) \land 0\text{xFF}$.
3. Read low byte of target address from zero-page location $\text{ptr}$.
4. Read high byte of target address from zero-page location $(\text{ptr} + 1) \land 0\text{xFF}$.
5. Effective Address $\text{addr\_abs} = (\text{high} \ll 8) \mid \text{low}$.

```text
Opcode byte: LDA ($20, X) where X = $04
Step 1: Base = $20
Step 2: Ptr = $20 + $04 = $24
Step 3: Low byte = Read($0024) -> $80
Step 4: High byte = Read($0025) -> $30
Step 5: Effective Address = $3080
```

### Implementation Code

::: code-group
```cpp [C++]
uint8_t CPU6502::IZX() {
    uint16_t t = read(pc++);

    // Zero-page indexed addition with 8-bit wraparound
    uint16_t lo_addr = (t + (uint16_t)x) & 0x00FF;
    uint16_t hi_addr = (t + (uint16_t)x + 1) & 0x00FF;

    uint16_t lo = read(lo_addr);
    uint16_t hi = read(hi_addr);

    addr_abs = (hi << 8) | lo;
    return 0; // Never adds extra cycle
}
```

```rust [Rust]
pub fn izx(&mut self) -> u8 {
    let t = self.read(self.pc) as u16;
    self.pc += 1;

    // Zero-page indexed addition with 8-bit wraparound
    let lo_addr = (t + (self.x as u16)) & 0x00FF;
    let hi_addr = (t + (self.x as u16) + 1) & 0x00FF;

    let lo = self.read(lo_addr) as u16;
    let hi = self.read(hi_addr) as u16;

    self.addr_abs = (hi << 8) | lo;
    0 // Never adds extra cycle
}
```

```typescript [TypeScript]
public IZX(): number {
  const t = this.read(this.pc++);

  // Zero-page indexed addition with 8-bit wraparound
  const loAddr = (t + this.x) & 0x00FF;
  const hiAddr = (t + this.x + 1) & 0x00FF;

  const lo = this.read(loAddr);
  const hi = this.read(hiAddr);

  this.addrAbs = (hi << 8) | lo;
  return 0; // Never adds extra cycle
}
```
:::

---

## 2. Resolving `($LL), Y` (Post-Indexed Indirect)

### Step-by-Step Execution
1. Fetch 8-bit base address $LL$ from instruction stream.
2. Read 16-bit base pointer directly from zero page:
   - $\text{low} = \text{Read}(LL \land 0\text{xFF})$
   - $\text{high} = \text{Read}((LL + 1) \land 0\text{xFF})$
3. $\text{base\_ptr} = (\text{high} \ll 8) \mid \text{low}$.
4. Add the `Y` register to the 16-bit base pointer: $\text{addr\_abs} = \text{base\_ptr} + Y$.
5. Check if adding `Y` crossed a 256-byte page boundary (i.e. $(\text{addr\_abs} \land 0\text{xFF00}) \neq (\text{high} \ll 8)$). If so, add 1 cycle!

```text
Opcode byte: LDA ($20), Y where Y = $10
Step 1: Base = $20
Step 2: Low byte = Read($0020) -> $00, High byte = Read($0021) -> $40 (Base Ptr = $4000)
Step 3: Effective Address = $4000 + $10 = $4010
```

### Implementation Code

::: code-group
```cpp [C++]
uint8_t CPU6502::IZY() {
    uint16_t t = read(pc++);

    // Read 16-bit pointer from Zero Page
    uint16_t lo = read(t & 0x00FF);
    uint16_t hi = read((t + 1) & 0x00FF);

    addr_abs = ((hi << 8) | lo) + y;

    // Check for page boundary crossing
    if ((addr_abs & 0xFF00) != (hi << 8)) {
        return 1; // Extra cycle candidate
    }
    return 0;
}
```

```rust [Rust]
pub fn izy(&mut self) -> u8 {
    let t = self.read(self.pc) as u16;
    self.pc += 1;

    // Read 16-bit pointer from Zero Page
    let lo = self.read(t & 0x00FF) as u16;
    let hi = self.read((t + 1) & 0x00FF) as u16;

    self.addr_abs = ((hi << 8) | lo).wrapping_add(self.y as u16);

    // Check for page boundary crossing
    if (self.addr_abs & 0xFF00) != (hi << 8) {
        1 // Extra cycle candidate
    } else {
        0
    }
}
```

```typescript [TypeScript]
public IZY(): number {
  const t = this.read(this.pc++);

  // Read 16-bit pointer from Zero Page
  const lo = this.read(t & 0x00FF);
  const hi = this.read((t + 1) & 0x00FF);

  this.addrAbs = (((hi << 8) | lo) + this.y) & 0xFFFF;

  // Check for page boundary crossing
  if ((this.addrAbs & 0xFF00) !== (hi << 8)) {
    return 1; // Extra cycle candidate
  }
  return 0;
}
```
:::
