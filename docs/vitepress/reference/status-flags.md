---
title: Status Register & Flags Reference
description: Detailed bit-level specification of the 6502 status register flags (NVUBDIZC) and their side effects.
---

The 6502 Status Register (also known as the **P** or **SR** register) is an 8-bit register holding status conditions resulting from ALU operations and CPU execution modes.

## Bit Layout

```text
  Bit   7   6   5   4   3   2   1   0
  Flag  N   V   U   B   D   I   Z   C
```

| Bit | Name | Mnemonic | Set Condition ($1$) | Cleared Condition ($0$) | Instructions Modifying |
| :---: | :--- | :---: | :--- | :--- | :--- |
| **7** | **Negative** | `N` | Result has bit 7 = 1 (negative in two's complement) | Result has bit 7 = 0 | ALU, Loads, Transfers, Shifts, Rotates, BIT, CMP |
| **6** | **Overflow** | `V` | Signed arithmetic overflow occurred, or bit 6 in BIT | No signed overflow | `ADC`, `SBC`, `BIT`, `CLV`, `PLP`, `RTI` |
| **5** | **Unused / Expansion** | `U` | Always pushed as 1 to stack | N/A (hardwired high in pushes) | None (Hardware fixed) |
| **4** | **Break** | `B` | Pushed by software interrupts (`BRK`, `PHP`) | Pushed by hardware interrupts (`IRQ`, `NMI`) | `BRK`, `PHP` |
| **3** | **Decimal Mode** | `D` | BCD arithmetic enabled | Standard binary arithmetic | `SED`, `CLD`, `PLP`, `RTI` |
| **2** | **Interrupt Disable** | `I` | Maskable interrupts (IRQ) ignored | IRQ interrupts accepted | `SEI`, `CLI`, `BRK`, `PLP`, `RTI` |
| **1** | **Zero** | `Z` | Result equals 0 | Result is non-zero | ALU, Loads, Transfers, Increments, Decrements, CMP |
| **0** | **Carry** | `C` | Arithmetic carry / no borrow / shift out 1 | Borrow occurred / shift out 0 | `ADC`, `SBC`, `SEC`, `CLC`, Shifts, Rotates, CMP |

---

## Detailed Flag Semantics

### Negative (`N`)
Reflects the most significant bit (MSB, bit 7) of any value loaded into a register (`A`, `X`, `Y`) or produced by the ALU. If the byte is viewed as a signed two's complement integer, `N = 1` means the number is negative ($-128$ to $-1$).

### Overflow (`V`)
Set during `ADC` and `SBC` when two signed 8-bit integers produce a result outside the valid $[-128, +127]$ range. Also loaded directly with bit 6 of the operand during a `BIT` instruction.

### Break Flag (`B`) & Unused (`U`)
The `B` flag **does not exist as a physical flip-flop inside the CPU core**. It only manifests when the Status Register is pushed onto the stack:
- In `PHP` and `BRK`: Bits 4 and 5 are pushed as `11` (status \| `0x30`).
- In hardware `IRQ` and `NMI`: Bits 4 and 5 are pushed as `01` (status \| `0x20`).
- When pulled by `PLP` or `RTI`, Bit 4 is completely ignored.

### Decimal Mode (`D`)
When set via `SED`, addition (`ADC`) and subtraction (`SBC`) treat each nibble as a Binary Coded Decimal digit ($0-9$). On the original NES 2A03 processor, the decimal hardware logic is severed and `D` has no effect on math.

### Interrupt Disable (`I`)
When set (`1`), maskable interrupt requests asserted on the `IRQ` pin will not trigger the interrupt sequence. Non-Maskable Interrupts (`NMI`) ignore `I`.

### Zero (`Z`)
Set whenever an arithmetic, logical, load, or transfer operation results in `0x00`.

### Carry (`C`)
Acts as the 9th bit of arithmetic operations:
- **Addition (`ADC`)**: Set if the sum exceeds 255 ($> 0\text{xFF}$).
- **Subtraction (`SBC`)**: Acts as an inverted borrow. Cleared if borrow is needed ($A < M$), set if no borrow is required ($A \ge M$).
- **Shifts & Rotates**: Receives the bit pushed off the edge.
