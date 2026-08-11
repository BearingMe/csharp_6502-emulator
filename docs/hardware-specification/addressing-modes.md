# 6502 Addressing Modes

Several addressing modes exist; not all instructions support all modes, and X/Y registers aren't always interchangeable. This lack of orthogonality makes the 6502 tricky to program well.

| Mode | Operand | Description |
|---|---|---|
| Implicit | none | Source/destination implied by the instruction itself. E.g. `CLC`, `RTS`. |
| Accumulator | `A` | Operates directly on the accumulator. E.g. `LSR A`, `ROR A`. |
| Immediate | `#nn` | 8-bit constant embedded in the instruction. E.g. `LDA #10`, `LDX #LO LABEL`. |
| Zero Page | `nn` | 8-bit address, range `$0000`-`$00FF` (MSB always 0). 1 byte shorter, 1 fewer memory fetch than absolute. Assembler auto-selects this mode when possible. E.g. `LDA $00`. |
| Zero Page,X | `nn,X` | Zero page address + X, **wraps within zero page** (no carry to page 1). E.g. `LDA $80,X`: X=`$0F` → `$008F`; X=`$FF` → `$007F` (not `$017F`). |
| Zero Page,Y | `nn,Y` | Same as Zero Page,X but with Y. **Only `LDX`/`STX` support this mode.** E.g. `LDX $10,Y`. |
| Relative | `label` | Signed 8-bit offset (-128 to +127) added to PC, used by branch instructions (`BEQ`, `BNE`, etc.). Since PC is already +2 when the offset applies, effective target range is -126 to +129 bytes from the branch. |
| Absolute | `nnnn` | Full 16-bit target address. E.g. `JMP $1234`, `JSR WIBBLE`. |
| Absolute,X | `nnnn,X` | 16-bit address + X (no wrap-around limit). E.g. `STA $2000,X` with X=`$92` → `$2092`. |
| Absolute,Y | `nnnn,Y` | Same as Absolute,X but with Y. E.g. `AND $4000,Y`. |
| Indirect | `(nnnn)` | **`JMP` only.** Instruction holds address of the LSB of the real 16-bit target (next byte = MSB). E.g. `$0120`=`$FC`, `$0121`=`$BA` → `JMP ($0120)` jumps to `$BAFC`. |
| Indexed Indirect | `(nn,X)` | Zero page table address + X (zero-page wrap) → points to LSB of target address. E.g. `LDA ($40,X)`. |
| Indirect Indexed | `(nn),Y` | Zero page location holds LSB of a 16-bit base address; Y is added to that address (not to the pointer) to get the target — most common indirection mode. E.g. `LDA ($40),Y`. |

## Notes

- **Zero Page,X/Y wrap-around**: sum stays within zero page even if it exceeds `$FF`.
- **Absolute,X/Y**: no such wrap — the full 16-bit sum is used.
- **Indexed Indirect vs Indirect Indexed**: X indexes *before* dereferencing (into the pointer table); Y indexes *after* dereferencing (into the target data).