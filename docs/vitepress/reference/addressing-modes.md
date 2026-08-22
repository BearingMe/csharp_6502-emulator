---
title: Addressing Modes Specification Reference
description: Formal specification of the 13 addressing modes supported by the MOS 6502 architecture.
---

The MOS 6502 supports 13 addressing modes to locate instruction operands in registers and across the 64KB address space.

## Addressing Modes Summary Table

| Mode Name | Acronym | Assembly Syntax | Bytes | Cycles | Effective Address Calculation |
| :--- | :---: | :--- | :---: | :---: | :--- |
| **Implied** | `IMP` | `TAX`, `CLC` | 1 | 2 | Target is internal CPU register or flag |
| **Accumulator** | `ACC` | `ASL A`, `ROR A` | 1 | 2 | Target is the Accumulator (`A`) |
| **Immediate** | `IMM` | `LDA #$44` | 2 | 2 | Operand is at `PC + 1` |
| **Zero Page** | `ZP0` | `LDA $44` | 2 | 3 | `Address = $00LL` |
| **Zero Page, X** | `ZPX` | `LDA $44,X` | 2 | 4 | `Address = $00((LL + X) & 0xFF)` |
| **Zero Page, Y** | `ZPY` | `LDX $44,Y` | 2 | 4 | `Address = $00((LL + Y) & 0xFF)` |
| **Relative** | `REL` | `BNE $04` | 2 | 2* | `Target = PC + 2 + signed(offset)` |
| **Absolute** | `ABS` | `LDA $4400` | 3 | 4 | `Address = $HHLL` (Little-endian in memory) |
| **Absolute, X** | `ABX` | `LDA $4400,X` | 3 | 4* | `Address = $HHLL + X` (+1 cycle on page cross) |
| **Absolute, Y** | `ABY` | `LDA $4400,Y` | 3 | 4* | `Address = $HHLL + Y` (+1 cycle on page cross) |
| **Indirect** | `IND` | `JMP ($4400)` | 3 | 5 | `Target = Word at ($HHLL)` (with page wrap bug) |
| **Indexed Indirect** | `IZX` | `LDA ($44,X)` | 2 | 6 | `Ptr = (LL + X) & 0xFF; Address = Word at ($00Ptr)` |
| **Indirect Indexed** | `IZY` | `LDA ($44),Y` | 2 | 5* | `Address = Word at ($00LL) + Y` (+1 on page cross) |

*\* Can require additional cycles under page-crossing conditions.*

---

## Detailed Addressing Specifications

### 1. Implied (`IMP`) & Accumulator (`ACC`)
Single-byte instructions where the target operand is hardcoded into the CPU decode logic.
- **Bytes**: 1
- **Cycles**: 2
- **Examples**: `NOP`, `CLC`, `SEC`, `PHA`, `PLA`, `ASL A`, `TXA`

### 2. Immediate (`IMM`)
The instruction provides a literal 8-bit constant immediately following the opcode.
- **Bytes**: 2
- **Cycles**: 2
- **Address**: `addr_abs = PC++`
- **Example**: `LDA #$10` (loads hex value `$10` into `A`)

### 3. Zero Page (`ZP0`)
Accesses memory within the first 256 bytes (`$0000` to `$00FF`). Requires only 1 operand byte.
- **Bytes**: 2
- **Cycles**: 3 (Reads/Stores) or 5 (Read-Modify-Write)
- **Address**: `addr_abs = read(PC++) & 0x00FF`
- **Example**: `STA $80` (stores `A` into `$0080`)

### 4. Zero Page Indexed (`ZPX` / `ZPY`)
Adds the index register (`X` or `Y`) to the 8-bit zero page address with **8-bit modular wraparound** (never leaves Page 0).
- **Bytes**: 2
- **Cycles**: 4
- **Address**: `addr_abs = (read(PC++) + X) & 0x00FF`
- **Note**: `ZPY` is only used with `LDX` and `STX`.

### 5. Absolute (`ABS`)
Provides a complete 16-bit address stored in little-endian order (`lo-byte`, `hi-byte`).
- **Bytes**: 3
- **Cycles**: 4 (Reads), 4 (Stores), 6 (Read-Modify-Write), 3 (`JMP`), 6 (`JSR`)
- **Address**: `lo = read(PC++); hi = read(PC++); addr_abs = (hi << 8) | lo;`

### 6. Absolute Indexed (`ABX` / `ABY`)
Adds the `X` or `Y` register to a 16-bit absolute base address.
- **Bytes**: 3
- **Cycles**: 4 base (+1 if page boundary crossed for reads)
- **Address**: `addr_abs = base + index`

### 7. Relative (`REL`)
Used exclusively by conditional branch instructions. Takes a signed two's complement offset ($-128$ to $+127$).
- **Bytes**: 2
- **Cycles**: 2 if branch not taken, 3 if taken, 4 if taken across a 256-byte page boundary.
- **Address**: `target = PC + 2 + signed_offset`

### 8. Indirect (`IND`)
Used only by `JMP ($xxxx)`. Reads the target address from the 16-bit pointer location.
- **Bytes**: 3
- **Cycles**: 5
- **Hardware Quirk**: If pointer is `$xxFF`, high byte is fetched from `$xx00` instead of `$xx00 + 0x100`.

### 9. Indexed Indirect (`IZX` / Pre-Indexed)
Operand is an 8-bit zero page base. The `X` register is added to the base with 8-bit wraparound. The effective 16-bit target address is read from zero page locations `(base + X)` and `(base + X + 1)`.
- **Bytes**: 2
- **Cycles**: 6

### 10. Indirect Indexed (`IZY` / Post-Indexed)
Operand is an 8-bit zero page base. A 16-bit pointer is read from `$00base` and `$00(base+1)`. The `Y` register is then added to that 16-bit pointer.
- **Bytes**: 2
- **Cycles**: 5 (+1 if adding `Y` crosses a page boundary for read operations)
