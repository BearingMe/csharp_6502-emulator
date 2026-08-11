# 6502 Opcodes

151 opcodes (legal/documented set), 56 instructions × addressing modes. Complements `6502-instruction-set.md`, `6502-registers.md`, `6502-addressing-modes.md` — mode names match the addressing-modes doc.

| Mnemonic | Mode | Opcode | Bytes | Cycles |
|---|---|---|---|---|
| ADC | Immediate | `$69` | 2 | 2 |
| ADC | Zero Page | `$65` | 2 | 3 |
| ADC | Zero Page,X | `$75` | 2 | 4 |
| ADC | Absolute | `$6D` | 3 | 4 |
| ADC | Absolute,X | `$7D` | 3 | 4* |
| ADC | Absolute,Y | `$79` | 3 | 4* |
| ADC | Indexed Indirect | `$61` | 2 | 6 |
| ADC | Indirect Indexed | `$71` | 2 | 5* |
| AND | Immediate | `$29` | 2 | 2 |
| AND | Zero Page | `$25` | 2 | 3 |
| AND | Zero Page,X | `$35` | 2 | 4 |
| AND | Absolute | `$2D` | 3 | 4 |
| AND | Absolute,X | `$3D` | 3 | 4* |
| AND | Absolute,Y | `$39` | 3 | 4* |
| AND | Indexed Indirect | `$21` | 2 | 6 |
| AND | Indirect Indexed | `$31` | 2 | 5* |
| ASL | Accumulator | `$0A` | 1 | 2 |
| ASL | Zero Page | `$06` | 2 | 5 |
| ASL | Zero Page,X | `$16` | 2 | 6 |
| ASL | Absolute | `$0E` | 3 | 6 |
| ASL | Absolute,X | `$1E` | 3 | 7 |
| BCC | Relative | `$90` | 2 | 2** |
| BCS | Relative | `$B0` | 2 | 2** |
| BEQ | Relative | `$F0` | 2 | 2** |
| BIT | Zero Page | `$24` | 2 | 3 |
| BIT | Absolute | `$2C` | 3 | 4 |
| BMI | Relative | `$30` | 2 | 2** |
| BNE | Relative | `$D0` | 2 | 2** |
| BPL | Relative | `$10` | 2 | 2** |
| BRK | Implicit | `$00` | 1 | 7 |
| BVC | Relative | `$50` | 2 | 2** |
| BVS | Relative | `$70` | 2 | 2** |
| CLC | Implicit | `$18` | 1 | 2 |
| CLD | Implicit | `$D8` | 1 | 2 |
| CLI | Implicit | `$58` | 1 | 2 |
| CLV | Implicit | `$B8` | 1 | 2 |
| CMP | Immediate | `$C9` | 2 | 2 |
| CMP | Zero Page | `$C5` | 2 | 3 |
| CMP | Zero Page,X | `$D5` | 2 | 4 |
| CMP | Absolute | `$CD` | 3 | 4 |
| CMP | Absolute,X | `$DD` | 3 | 4* |
| CMP | Absolute,Y | `$D9` | 3 | 4* |
| CMP | Indexed Indirect | `$C1` | 2 | 6 |
| CMP | Indirect Indexed | `$D1` | 2 | 5* |
| CPX | Immediate | `$E0` | 2 | 2 |
| CPX | Zero Page | `$E4` | 2 | 3 |
| CPX | Absolute | `$EC` | 3 | 4 |
| CPY | Immediate | `$C0` | 2 | 2 |
| CPY | Zero Page | `$C4` | 2 | 3 |
| CPY | Absolute | `$CC` | 3 | 4 |
| DEC | Zero Page | `$C6` | 2 | 5 |
| DEC | Zero Page,X | `$D6` | 2 | 6 |
| DEC | Absolute | `$CE` | 3 | 6 |
| DEC | Absolute,X | `$DE` | 3 | 7 |
| DEX | Implicit | `$CA` | 1 | 2 |
| DEY | Implicit | `$88` | 1 | 2 |
| EOR | Immediate | `$49` | 2 | 2 |
| EOR | Zero Page | `$45` | 2 | 3 |
| EOR | Zero Page,X | `$55` | 2 | 4 |
| EOR | Absolute | `$4D` | 3 | 4 |
| EOR | Absolute,X | `$5D` | 3 | 4* |
| EOR | Absolute,Y | `$59` | 3 | 4* |
| EOR | Indexed Indirect | `$41` | 2 | 6 |
| EOR | Indirect Indexed | `$51` | 2 | 5* |
| INC | Zero Page | `$E6` | 2 | 5 |
| INC | Zero Page,X | `$F6` | 2 | 6 |
| INC | Absolute | `$EE` | 3 | 6 |
| INC | Absolute,X | `$FE` | 3 | 7 |
| INX | Implicit | `$E8` | 1 | 2 |
| INY | Implicit | `$C8` | 1 | 2 |
| JMP | Absolute | `$4C` | 3 | 3 |
| JMP | Indirect | `$6C` | 3 | 5*** |
| JSR | Absolute | `$20` | 3 | 6 |
| LDA | Immediate | `$A9` | 2 | 2 |
| LDA | Zero Page | `$A5` | 2 | 3 |
| LDA | Zero Page,X | `$B5` | 2 | 4 |
| LDA | Absolute | `$AD` | 3 | 4 |
| LDA | Absolute,X | `$BD` | 3 | 4* |
| LDA | Absolute,Y | `$B9` | 3 | 4* |
| LDA | Indexed Indirect | `$A1` | 2 | 6 |
| LDA | Indirect Indexed | `$B1` | 2 | 5* |
| LDX | Immediate | `$A2` | 2 | 2 |
| LDX | Zero Page | `$A6` | 2 | 3 |
| LDX | Zero Page,Y | `$B6` | 2 | 4 |
| LDX | Absolute | `$AE` | 3 | 4 |
| LDX | Absolute,Y | `$BE` | 3 | 4* |
| LDY | Immediate | `$A0` | 2 | 2 |
| LDY | Zero Page | `$A4` | 2 | 3 |
| LDY | Zero Page,X | `$B4` | 2 | 4 |
| LDY | Absolute | `$AC` | 3 | 4 |
| LDY | Absolute,X | `$BC` | 3 | 4* |
| LSR | Accumulator | `$4A` | 1 | 2 |
| LSR | Zero Page | `$46` | 2 | 5 |
| LSR | Zero Page,X | `$56` | 2 | 6 |
| LSR | Absolute | `$4E` | 3 | 6 |
| LSR | Absolute,X | `$5E` | 3 | 7 |
| NOP | Implicit | `$EA` | 1 | 2 |
| ORA | Immediate | `$09` | 2 | 2 |
| ORA | Zero Page | `$05` | 2 | 3 |
| ORA | Zero Page,X | `$15` | 2 | 4 |
| ORA | Absolute | `$0D` | 3 | 4 |
| ORA | Absolute,X | `$1D` | 3 | 4* |
| ORA | Absolute,Y | `$19` | 3 | 4* |
| ORA | Indexed Indirect | `$01` | 2 | 6 |
| ORA | Indirect Indexed | `$11` | 2 | 5* |
| PHA | Implicit | `$48` | 1 | 3 |
| PHP | Implicit | `$08` | 1 | 3 |
| PLA | Implicit | `$68` | 1 | 4 |
| PLP | Implicit | `$28` | 1 | 4 |
| ROL | Accumulator | `$2A` | 1 | 2 |
| ROL | Zero Page | `$26` | 2 | 5 |
| ROL | Zero Page,X | `$36` | 2 | 6 |
| ROL | Absolute | `$2E` | 3 | 6 |
| ROL | Absolute,X | `$3E` | 3 | 7 |
| ROR | Accumulator | `$6A` | 1 | 2 |
| ROR | Zero Page | `$66` | 2 | 5 |
| ROR | Zero Page,X | `$76` | 2 | 6 |
| ROR | Absolute | `$6E` | 3 | 6 |
| ROR | Absolute,X | `$7E` | 3 | 7 |
| RTI | Implicit | `$40` | 1 | 6 |
| RTS | Implicit | `$60` | 1 | 6 |
| SBC | Immediate | `$E9` | 2 | 2 |
| SBC | Zero Page | `$E5` | 2 | 3 |
| SBC | Zero Page,X | `$F5` | 2 | 4 |
| SBC | Absolute | `$ED` | 3 | 4 |
| SBC | Absolute,X | `$FD` | 3 | 4* |
| SBC | Absolute,Y | `$F9` | 3 | 4* |
| SBC | Indexed Indirect | `$E1` | 2 | 6 |
| SBC | Indirect Indexed | `$F1` | 2 | 5* |
| SEC | Implicit | `$38` | 1 | 2 |
| SED | Implicit | `$F8` | 1 | 2 |
| SEI | Implicit | `$78` | 1 | 2 |
| STA | Zero Page | `$85` | 2 | 3 |
| STA | Zero Page,X | `$95` | 2 | 4 |
| STA | Absolute | `$8D` | 3 | 4 |
| STA | Absolute,X | `$9D` | 3 | 5 |
| STA | Absolute,Y | `$99` | 3 | 5 |
| STA | Indexed Indirect | `$81` | 2 | 6 |
| STA | Indirect Indexed | `$91` | 2 | 6 |
| STX | Zero Page | `$86` | 2 | 3 |
| STX | Zero Page,Y | `$96` | 2 | 4 |
| STX | Absolute | `$8E` | 3 | 4 |
| STY | Zero Page | `$84` | 2 | 3 |
| STY | Zero Page,X | `$94` | 2 | 4 |
| STY | Absolute | `$8C` | 3 | 4 |
| TAX | Implicit | `$AA` | 1 | 2 |
| TAY | Implicit | `$A8` | 1 | 2 |
| TSX | Implicit | `$BA` | 1 | 2 |
| TXA | Implicit | `$8A` | 1 | 2 |
| TXS | Implicit | `$9A` | 1 | 2 |
| TYA | Implicit | `$98` | 1 | 2 |

## Notes

- **\*** +1 cycle if a page boundary is crossed (indexed addressing where the high byte of the effective address changes).
- **\*\*** Branch cycle count: 2 if not taken; +1 if taken (same page); +2 if taken to a different page.
- **\*\*\*** `JMP (abs)` has the page-boundary bug on NMOS 6502: if the pointer's low byte is `$FF`, the high byte is fetched from the start of the *same* page instead of the next one (e.g. `JMP ($11FF)` reads high byte from `$1100`, not `$1200`). Fixed on 65C02 (costs +1 cycle there instead).
- **STA has no page-cross penalty**: unlike loads/ALU ops, `STA abs,X`/`STA abs,Y`/`STA (zp),Y` always take the indexed-mode cycle count — a write can't skip the dummy read that resolves the page crossing.
- **Unofficial/illegal opcodes** (the remaining 105 byte values, e.g. `LAX`, `SAX`, `DCP`, `SLO`, NOP variants) are NMOS-specific side effects of incomplete opcode decoding — undocumented, unstable on some, and not present on 65C02. Not included here; flag if needed separately.
- Source cross-checked against masswerk.at 6502 instruction set reference.