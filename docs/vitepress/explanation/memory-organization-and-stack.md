---
title: Memory Organization, Pages, & Stack Mechanics
description: Deep dive into the 6502 64KB memory structure, Zero Page optimization, and the Page 1 hardware stack.
---

The MOS 6502 architecture organizes its 16-bit address space into 256 "pages" of 256 bytes each ($256 \times 256 = 65,536$ bytes).

## Memory Map Breakdown

| Page Number | Address Range | Primary Hardware Function |
| :---: | :--- | :--- |
| **Page 0** | `$0000 - $00FF` | **Zero Page**: High-speed memory accessed with 1-byte addresses. |
| **Page 1** | `$0100 - $01FF` | **Hardware Stack**: Descending LIFO stack indexed by the 8-bit `SP` register. |
| **Pages 2–7** | `$0200 - $07FF` | Internal RAM / Display buffers. |
| **Pages 8–254**| `$0800 - $DFEF` | Expansion RAM, Cartridge ROM, Memory-Mapped I/O registers. |
| **Page 255** | `$FF00 - $FFFF` | Top of memory containing **Hardware Interrupt Vectors** (`$FFFA-$FFFF`). |

---

## Why Zero Page Matters

On modern CPUs, registers provide the fastest storage. Because the 6502 only possesses three general registers (`A`, `X`, `Y`), the designers treated Page 0 as **256 quasi-registers**:

1. **Smaller Code Size**: Zero Page instructions require only a 1-byte address operand (e.g. `LDA $80` takes 2 bytes total instead of 3 bytes for `LDA $0080`).
2. **Faster Execution**: Saving 1 byte of opcode fetch saves **1 CPU clock cycle** per memory access.
3. **Indirect Pointers**: The powerful indirect addressing modes `($LL, X)` and `($LL), Y` require base pointers to reside in the Zero Page.

---

## Page 1 Hardware Stack Architecture

The 6502 stack is hardwired to addresses `$0100` through `$01FF`:

- **Stack Pointer (`SP`)**: An 8-bit register holding the offset from `$0100`.
- **Top of Stack**: Located at `0x0100 + SP`.
- **Growth Direction**: **Descending** (downward).
  - Pushing a byte writes to `0x0100 + SP`, then decrements `SP--`.
  - Pulling a byte increments `++SP`, then reads from `0x0100 + SP`.
- **Wraparound**: If `SP` decrements past `0x00`, it wraps to `0xFF` within Page 1 (stack underflow/overflow never corrupts Page 0 or Page 2).
- **Initial Reset Value**: Hardware initializes `SP` to `0xFD` (after consuming 3 dummy pushes during the reset sequence).
