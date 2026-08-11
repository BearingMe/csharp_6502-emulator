# 6502 Registers

The 6502 has few registers vs. peers of its era; efficient register/memory use is critical.

## Registers

| Register | Size | Purpose |
|---|---|---|
| Program Counter (PC) | 16-bit | Points to next instruction. Auto-updates on execution; also changed by jump, branch, subroutine call, or return from subroutine/interrupt. |
| Stack Pointer (SP) | 8-bit | Low byte of next free stack slot. Stack is fixed at `$0100`-`$01FF`. Push decrements SP, pull increments SP. No overflow detection — overflow likely crashes the program. |
| Accumulator (A) | 8-bit | Used for all arithmetic/logic ops except INC/DEC. Can be stored/loaded via memory or stack. Primary target for optimization in time-critical code. |
| Index X | 8-bit | Holds counters/offsets. Supports load, store, compare, INC/DEC. Special: can copy to/from SP. |
| Index Y | 8-bit | Same as X (load, store, compare, INC/DEC) but no special functions. |

## Processor Status Flags

Single-bit flags in the status register, set/cleared by instruction results. Can be tested, set, cleared, or pushed/pulled as a group.

| Flag | Set when | Related instructions |
|---|---|---|
| Carry (C) | Overflow from bit 7, or underflow from bit 0, in arithmetic/comparison/shift ops | `SEC` set, `CLC` clear |
| Zero (Z) | Result of last op == 0 | — |
| Interrupt Disable (I) | CPU ignores interrupts until cleared | `SEI` set, `CLI` clear |
| Decimal Mode (D) | Arithmetic follows BCD rules | `SED` set, `CLD` clear |
| Break (B) | `BRK` executed, interrupt generated | — |
| Overflow (V) | Invalid two's-complement result (e.g., 64+64 → -128); derived from carry between bits 6/7 and bit 7/carry | — |
| Negative (N) | Bit 7 of result == 1 | — |