# 6502 Instruction Set

56 instructions across 11 functional groups.

| Group | Instr | Description | Flags |
|---|---|---|---|
| Load/Store | `LDA` | Load Accumulator | N,Z |
| Load/Store | `LDX` | Load X Register | N,Z |
| Load/Store | `LDY` | Load Y Register | N,Z |
| Load/Store | `STA` | Store Accumulator | — |
| Load/Store | `STX` | Store X Register | — |
| Load/Store | `STY` | Store Y Register | — |
| Transfer | `TAX` | Transfer accumulator to X | N,Z |
| Transfer | `TAY` | Transfer accumulator to Y | N,Z |
| Transfer | `TXA` | Transfer X to accumulator | N,Z |
| Transfer | `TYA` | Transfer Y to accumulator | N,Z |
| Stack | `TSX` | Transfer stack pointer to X | N,Z |
| Stack | `TXS` | Transfer X to stack pointer | — |
| Stack | `PHA` | Push accumulator on stack | — |
| Stack | `PHP` | Push processor status on stack | — |
| Stack | `PLA` | Pull accumulator from stack | N,Z |
| Stack | `PLP` | Pull processor status from stack | All |
| Logical | `AND` | Logical AND | N,Z |
| Logical | `EOR` | Exclusive OR | N,Z |
| Logical | `ORA` | Logical Inclusive OR | N,Z |
| Logical | `BIT` | Bit Test (AND for flags only; result discarded) | N,V,Z |
| Arithmetic | `ADC` | Add with Carry | N,V,Z,C |
| Arithmetic | `SBC` | Subtract with Carry | N,V,Z,C |
| Arithmetic | `CMP` | Compare accumulator | N,Z,C |
| Arithmetic | `CPX` | Compare X register | N,Z,C |
| Arithmetic | `CPY` | Compare Y register | N,Z,C |
| Inc/Dec | `INC` | Increment a memory location | N,Z |
| Inc/Dec | `INX` | Increment the X register | N,Z |
| Inc/Dec | `INY` | Increment the Y register | N,Z |
| Inc/Dec | `DEC` | Decrement a memory location | N,Z |
| Inc/Dec | `DEX` | Decrement the X register | N,Z |
| Inc/Dec | `DEY` | Decrement the Y register | N,Z |
| Shifts | `ASL` | Arithmetic Shift Left | N,Z,C |
| Shifts | `LSR` | Logical Shift Right | N,Z,C |
| Shifts | `ROL` | Rotate Left (through carry) | N,Z,C |
| Shifts | `ROR` | Rotate Right (through carry) | N,Z,C |
| Jumps/Calls | `JMP` | Jump to another location | — |
| Jumps/Calls | `JSR` | Jump to subroutine (pushes return addr) | — |
| Jumps/Calls | `RTS` | Return from subroutine | — |
| Branches | `BCC` | Branch if carry clear | — |
| Branches | `BCS` | Branch if carry set | — |
| Branches | `BEQ` | Branch if zero set | — |
| Branches | `BMI` | Branch if negative set | — |
| Branches | `BNE` | Branch if zero clear | — |
| Branches | `BPL` | Branch if negative clear | — |
| Branches | `BVC` | Branch if overflow clear | — |
| Branches | `BVS` | Branch if overflow set | — |
| Flag Changes | `CLC` | Clear carry flag | C |
| Flag Changes | `CLD` | Clear decimal mode flag | D |
| Flag Changes | `CLI` | Clear interrupt disable flag | I |
| Flag Changes | `CLV` | Clear overflow flag | V |
| Flag Changes | `SEC` | Set carry flag | C |
| Flag Changes | `SED` | Set decimal mode flag | D |
| Flag Changes | `SEI` | Set interrupt disable flag | I |
| System | `BRK` | Force an interrupt | B |
| System | `NOP` | No operation | — |
| System | `RTI` | Return from interrupt | All |

## Notes

- **Load/Store**: loads set N,Z from the transferred value; stores don't affect flags.
- **Stack**: 256-byte stack, fixed `$0100`-`$01FF`. S register tracks next free byte (`$0100,S`); push = store then decrement, pull = increment then load. S is only accessible via X (`TSX`/`TXS`) — never directly.
- **Logical/BIT**: `BIT` ANDs accumulator with memory to set N,V,Z but discards the result (accumulator unchanged).
- **Shifts vs Rotates**: `ASL`/`LSR` shift in a 0; `ROL`/`ROR` shift in the current carry bit. All four catch the bit shifted out in carry (C).
- **JSR/RTS**: `JSR` pushes the return address before jumping, so a later `RTS` resumes after the call.
- **Branches**: use a signed 8-bit relative offset — target must be within -126/+128 bytes of the branch instruction.