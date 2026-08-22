---
title: Complete 6502 Instruction Set Matrix
description: Full 16x16 opcode map and instruction index for the MOS 6502 microprocessor.
---

The MOS 6502 uses an 8-bit instruction opcode ($256$ total slots). Below is the complete opcode lookup matrix indexed by upper nibble (row) and lower nibble (column).

## Opcode Matrix (16x16)

| Hi \ Lo | -0 | -1 | -2 | -3 | -4 | -5 | -6 | -7 | -8 | -9 | -A | -B | -C | -D | -E | -F |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **0-** | BRK | ORA (izx) | * | * | * | ORA zp | ASL zp | * | PHP | ORA # | ASL A | * | * | ORA abs | ASL abs | * |
| **1-** | BPL | ORA (izy) | * | * | * | ORA zpx | ASL zpx | * | CLC | ORA aby | * | * | * | ORA abx | ASL abx | * |
| **2-** | JSR | AND (izx) | * | * | BIT zp | AND zp | ROL zp | * | PLP | AND # | ROL A | * | BIT abs | AND abs | ROL abs | * |
| **3-** | BMI | AND (izy) | * | * | * | AND zpx | ROL zpx | * | SEC | AND aby | * | * | * | AND abx | ROL abx | * |
| **4-** | RTI | EOR (izx) | * | * | * | EOR zp | LSR zp | * | PHA | EOR # | LSR A | * | JMP abs | EOR abs | LSR abs | * |
| **5-** | BVC | EOR (izy) | * | * | * | EOR zpx | LSR zpx | * | CLI | EOR aby | * | * | * | EOR abx | LSR abx | * |
| **6-** | RTS | ADC (izx) | * | * | * | ADC zp | ROR zp | * | PLA | ADC # | ROR A | * | JMP ind | ADC abs | ROR abs | * |
| **7-** | BVS | ADC (izy) | * | * | * | ADC zpx | ROR zpx | * | SEI | ADC aby | * | * | * | ADC abx | ROR abx | * |
| **8-** | * | STA (izx) | * | * | STY zp | STA zp | STX zp | * | DEY | * | TXA | * | STY abs | STA abs | STX abs | * |
| **9-** | BCC | STA (izy) | * | * | STY zpx | STA zpx | STX zpy | * | TYA | STA aby | TXS | * | * | STA abx | * | * |
| **A-** | LDY # | LDA (izx) | LDX # | * | LDY zp | LDA zp | LDX zp | * | TAY | LDA # | TAX | * | LDY abs | LDA abs | LDX abs | * |
| **B-** | BCS | LDA (izy) | * | * | LDY zpx | LDA zpx | LDX zpy | * | CLV | LDA aby | TSX | * | LDY abx | LDA abx | LDX aby | * |
| **C-** | CPY # | CMP (izx) | * | * | CPY zp | CMP zp | DEC zp | * | INY | CMP # | DEX | * | CPY abs | CMP abs | DEC abs | * |
| **D-** | BNE | CMP (izy) | * | * | * | CMP zpx | DEC zpx | * | CLD | CMP aby | * | * | * | CMP abx | DEC abx | * |
| **E-** | CPX # | SBC (izx) | * | * | CPX zp | SBC zp | INC zp | * | INX | SBC # | NOP | * | CPX abs | SBC abs | INC abs | * |
| **F-** | BEQ | SBC (izy) | * | * | * | SBC zpx | INC zpx | * | SED | SBC aby | * | * | * | SBC abx | INC abx | * |

*\* Unofficial / Undocumented opcodes*

---

## 56 Official Instructions Alphabetical Reference

| Mnemonic | Full Name | Addressing Modes Supported | Flags Affected |
| :--- | :--- | :--- | :---: |
| **ADC** | Add with Carry | IMM, ZP0, ZPX, ABS, ABX, ABY, IZX, IZY | N, Z, C, V |
| **AND** | Logical AND with Accumulator | IMM, ZP0, ZPX, ABS, ABX, ABY, IZX, IZY | N, Z |
| **ASL** | Arithmetic Shift Left | ACC, ZP0, ZPX, ABS, ABX | N, Z, C |
| **BCC** | Branch if Carry Clear | REL | None |
| **BCS** | Branch if Carry Set | REL | None |
| **BEQ** | Branch if Equal ($Z=1$) | REL | None |
| **BIT** | Bit Test | ZP0, ABS | N, Z, V |
| **BMI** | Branch if Minus ($N=1$) | REL | None |
| **BNE** | Branch if Not Equal ($Z=0$) | REL | None |
| **BPL** | Branch if Plus ($N=0$) | REL | None |
| **BRK** | Force Break / Software Interrupt | IMP | B, I |
| **BVC** | Branch if Overflow Clear ($V=0$) | REL | None |
| **BVS** | Branch if Overflow Set ($V=1$) | REL | None |
| **CLC** | Clear Carry Flag | IMP | C |
| **CLD** | Clear Decimal Mode | IMP | D |
| **CLI** | Clear Interrupt Disable | IMP | I |
| **CLV** | Clear Overflow Flag | IMP | V |
| **CMP** | Compare Accumulator | IMM, ZP0, ZPX, ABS, ABX, ABY, IZX, IZY | N, Z, C |
| **CPX** | Compare X Register | IMM, ZP0, ABS | N, Z, C |
| **CPY** | Compare Y Register | IMM, ZP0, ABS | N, Z, C |
| **DEC** | Decrement Memory | ZP0, ZPX, ABS, ABX | N, Z |
| **DEX** | Decrement X Register | IMP | N, Z |
| **DEY** | Decrement Y Register | IMP | N, Z |
| **EOR** | Exclusive OR with Accumulator | IMM, ZP0, ZPX, ABS, ABX, ABY, IZX, IZY | N, Z |
| **INC** | Increment Memory | ZP0, ZPX, ABS, ABX | N, Z |
| **INX** | Increment X Register | IMP | N, Z |
| **INY** | Increment Y Register | IMP | N, Z |
| **JMP** | Jump to Address | ABS, IND | None |
| **JSR** | Jump to Subroutine | ABS | None |
| **LDA** | Load Accumulator | IMM, ZP0, ZPX, ABS, ABX, ABY, IZX, IZY | N, Z |
| **LDX** | Load X Register | IMM, ZP0, ZPY, ABS, ABY | N, Z |
| **LDY** | Load Y Register | IMM, ZP0, ZPX, ABS, ABX | N, Z |
| **LSR** | Logical Shift Right | ACC, ZP0, ZPX, ABS, ABX | N, Z, C |
| **NOP** | No Operation | IMP | None |
| **ORA** | Logical OR with Accumulator | IMM, ZP0, ZPX, ABS, ABX, ABY, IZX, IZY | N, Z |
| **PHA** | Push Accumulator to Stack | IMP | None |
| **PHP** | Push Processor Status to Stack | IMP | None |
| **PLA** | Pull Accumulator from Stack | IMP | N, Z |
| **PLP** | Pull Processor Status from Stack | IMP | N, V, D, I, Z, C |
| **ROL** | Rotate Left through Carry | ACC, ZP0, ZPX, ABS, ABX | N, Z, C |
| **ROR** | Rotate Right through Carry | ACC, ZP0, ZPX, ABS, ABX | N, Z, C |
| **RTI** | Return from Interrupt | IMP | All Flags |
| **RTS** | Return from Subroutine | IMP | None |
| **SBC** | Subtract with Borrow | IMM, ZP0, ZPX, ABS, ABX, ABY, IZX, IZY | N, Z, C, V |
| **SEC** | Set Carry Flag | IMP | C |
| **SED** | Set Decimal Mode Flag | IMP | D |
| **SEI** | Set Interrupt Disable Flag | IMP | I |
| **STA** | Store Accumulator | ZP0, ZPX, ABS, ABX, ABY, IZX, IZY | None |
| **STX** | Store X Register | ZP0, ZPY, ABS | None |
| **STY** | Store Y Register | ZP0, ZPX, ABS | None |
| **TAX** | Transfer Accumulator to X | IMP | N, Z |
| **TAY** | Transfer Accumulator to Y | IMP | N, Z |
| **TSX** | Transfer Stack Pointer to X | IMP | N, Z |
| **TXA** | Transfer X to Accumulator | IMP | N, Z |
| **TXS** | Transfer X to Stack Pointer | IMP | None |
| **TYA** | Transfer Y to Accumulator | IMP | N, Z |
