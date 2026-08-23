using System.IO.Compression;
using System.Numerics;

namespace mos6502;

public class Emulator
{
  private readonly Bus _bus;
  private u8 _a;
  private u8 _x;
  private u8 _y;
  private u8 _stkp;
  private u16 _pc;
  private Status _status;

  public u8 A => _a;
  public u8 X => _x;
  public u8 Y => _y;
  public u8 StackPointer => _stkp;
  public Status Status => _status;

  public Emulator(Bus bus)
  {
    _bus = bus;
    _a = 0x00;
    _x = 0x00;
    _y = 0x00;
    _stkp = 0xFD;
    _pc = _bus.ReadWord(0xFFFC);
    _status = 0x00 | Status.Interrupt;
  }

  public cycle LDA_immediate(u8 operand)
  {
    var value = operand;

    UpdateZNFlags(value);

    _a = value;

    return 2;
  }

  public cycle LDA_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);

    UpdateZNFlags(value);

    _a = value;

    return 3;
  }

  public cycle LDA_zero_page_x(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _x));

    UpdateZNFlags(value);

    _a = value;

    return 4;
  }

  public cycle LDA_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);

    UpdateZNFlags(value);

    _a = value;

    return 4;
  }

  public cycle LDA_absolute_x(u16 operand)
  {
    var temp = operand + _x;
    var value = _bus.ReadByte((u16)(temp));

    UpdateZNFlags(value);

    _a = value;

    return HasPageCrossed(operand, temp) ? 5 : 4;
  }

  public cycle LDA_absolute_y(u16 operand)
  {
    var temp = operand + _y;
    var value = _bus.ReadByte((u16)(temp));

    UpdateZNFlags(value);

    _a = value;

    return HasPageCrossed(operand, temp) ? 5 : 4;
  }

  public cycle LDA_indexed_indirect(u8 operand)
  {
    var lo = _bus.ReadByte((u8)(operand + _x));
    var hi = _bus.ReadByte((u8)(operand + _x + 1));
    var pointer = CombineBytesToWord(lo, hi);

    var value = _bus.ReadByte(pointer);

    UpdateZNFlags(value);

    _a = value;

    return 6;
  }

  public cycle LDA_indirect_indexed(u8 operand)
  {
    var lo = _bus.ReadByte(operand);
    var hi = _bus.ReadByte((u8)(operand + 1));
    var pointer = CombineBytesToWord(lo, hi);
    var address = (u16)(pointer + _y);
    var value = _bus.ReadByte(address);

    UpdateZNFlags(value);

    _a = value;

    return HasPageCrossed(pointer, address) ? 6 : 5;
  }

  public cycle LDX_immediate(u8 operand)
  {
    var value = operand;

    UpdateZNFlags(value);

    _x = value;

    return 2;
  }

  public cycle LDX_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);

    UpdateZNFlags(value);

    _x = value;

    return 3;
  }

  public cycle LDX_zero_page_y(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _y));

    UpdateZNFlags(value);

    _x = value;

    return 4;
  }

  public cycle LDX_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);

    UpdateZNFlags(value);

    _x = value;

    return 4;
  }

  public cycle LDX_absolute_y(u16 operand)
  {
    var temp = operand + _y;
    var value = _bus.ReadByte((u16)temp);

    UpdateZNFlags(value);

    _x = value;

    return HasPageCrossed(operand, temp) ? 5 : 4;
  }

  public cycle LDY_immediate(u8 operand)
  {
    var value = operand;

    UpdateZNFlags(value);

    _y = value;

    return 2;
  }

  public cycle LDY_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);

    UpdateZNFlags(value);

    _y = value;

    return 3;
  }

  public cycle LDY_zero_page_x(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _x));

    UpdateZNFlags(value);

    _y = value;

    return 4;
  }

  public cycle LDY_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);

    UpdateZNFlags(value);

    _y = value;

    return 4;
  }

  public cycle LDY_absolute_x(u16 operand)
  {
    var temp = operand + _x;
    var value = _bus.ReadByte((u16)temp);

    UpdateZNFlags(value);

    _y = value;

    return HasPageCrossed(operand, temp) ? 5 : 4;
  }

  public cycle STA_zero_page(u8 operand)
  {
    _bus.WriteByte(operand, _a);
    return 3;
  }

  public cycle STA_zero_page_x(u8 operand)
  {
    _bus.WriteByte((u8)(operand + _x), _a);
    return 4;
  }

  public cycle STA_absolute(u16 operand)
  {
    _bus.WriteByte(operand, _a);
    return 4;
  }

  public cycle STA_absolute_x(u16 operand)
  {
    var address = operand + _x;
    _bus.WriteByte((u16)address, _a);
    return HasPageCrossed(address, operand) ? 5 : 4;
  }

  public cycle STA_absolute_y(u16 operand)
  {
    var address = operand + _y;
    _bus.WriteByte((u16)address, _a);
    return HasPageCrossed(address, operand) ? 5 : 4;
  }

  public cycle STA_indexed_indirect(u8 operand)
  {
    var lo = _bus.ReadByte((u8)(operand + _x));
    var hi = _bus.ReadByte((u8)(operand + _x + 1));
    var address = CombineBytesToWord(lo, hi);

    _bus.WriteByte(address, _a);

    return 6;
  }

  public cycle STA_indirect_indexed(u8 operand)
  {
    var lo = _bus.ReadByte(operand);
    var hi = _bus.ReadByte((u8)(operand + 1));
    var pointer = CombineBytesToWord(lo, hi);
    var address = (u16)(pointer + _y);

    _bus.WriteByte(address, _a);


    return HasPageCrossed(pointer, address) ? 6 : 5;
  }

  public cycle STX_zero_page(u8 operand)
  {
    _bus.WriteByte(operand, _x);
    return 3;
  }

  public cycle STX_zero_page_y(u8 operand)
  {
    _bus.WriteByte((u8)(operand + _y), _x);
    return 4;
  }

  public cycle STX_absolute(u16 operand)
  {
    _bus.WriteByte(operand, _x);
    return 4;
  }

  public cycle STY_zero_page(u8 operand)
  {
    _bus.WriteByte(operand, _y);
    return 3;
  }

  public cycle STY_zero_page_x(u8 operand)
  {
    _bus.WriteByte((u8)(operand + _x), _y);
    return 4;
  }

  public cycle STY_absolute(u16 operand)
  {
    _bus.WriteByte(operand, _y);
    return 4;
  }


  public cycle TAX()
  {
    _x = _a;

    UpdateZNFlags(_x);

    return 2;
  }

  public cycle TAY()
  {
    _y = _a;

    UpdateZNFlags(_y);

    return 2;
  }

  public cycle TSX()
  {
    _x = _stkp;

    UpdateZNFlags(_x);

    return 2;
  }

  public cycle TXA()
  {
    _a = _x;

    UpdateZNFlags(_a);

    return 2;
  }

  public cycle TXS()
  {
    _stkp = _x;

    return 2;
  }

  public cycle TYA()
  {
    _a = _y;

    UpdateZNFlags(_a);

    return 2;
  }

  public cycle INX()
  {
    _x = (u8)(_x + 1);
    UpdateZNFlags(_x);

    return 2;
  }

  public cycle INY()
  {
    _y = (u8)(_y + 1);
    UpdateZNFlags(_y);

    return 2;
  }

  public cycle ADC_immediate(u8 operand)
  {
    var tempInt = _a + operand + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ operand) & 0x80));
    UpdateZNFlags(tempByte);

    _a = (u8)tempInt;

    return 2;
  }

  public cycle ADC_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);
    var tempInt = _a + value + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ value) & 0x80));
    UpdateZNFlags(tempByte);

    _a = (u8)tempInt;

    return 3;
  }

  public cycle ADC_zero_page_x(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _x));
    var tempInt = _a + value + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ value) & 0x80));
    UpdateZNFlags(tempByte);

    _a = (u8)tempInt;

    return 4;
  }

  public cycle ADC_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);
    var tempInt = _a + value + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ value) & 0x80));
    UpdateZNFlags(tempByte);

    _a = (u8)tempInt;

    return 4;
  }

  public cycle ADC_absolute_x(u16 operand)
  {
    var value = _bus.ReadByte((u16)(operand + _x));
    var tempInt = _a + value + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ value) & 0x80));
    UpdateZNFlags(tempByte);

    _a = (u8)tempInt;

    return HasPageCrossed(operand + _x, operand) ? 5 : 4;
  }

  public cycle ADC_absolute_y(u16 operand)
  {
    var value = _bus.ReadByte((u16)(operand + _y));
    var tempInt = _a + value + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ value) & 0x80));
    UpdateZNFlags(tempByte);

    _a = (u8)tempInt;

    return HasPageCrossed(operand + _y, operand) ? 5 : 4;
  }

  public cycle ADC_indexed_indirect(u8 operand)
  {
    var lo = _bus.ReadByte((u8)(operand + _x));
    var hi = _bus.ReadByte((u8)(operand + _x + 1));
    var pointer = CombineBytesToWord(lo, hi);
    var value = _bus.ReadByte(pointer);

    var tempInt = _a + value + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (u8)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ value) & 0x80));
    UpdateZNFlags(tempByte);

    _a = tempByte;

    return 6;
  }

  public cycle ADC_indirect_indexed(u8 operand)
  {
    var lo = _bus.ReadByte(operand);
    var hi = _bus.ReadByte((u8)(operand + 1));
    var pointer = CombineBytesToWord(lo, hi);
    var address = (u16)(pointer + _y);
    var value = _bus.ReadByte(address);

    var tempInt = _a + value + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ value) & 0x80));
    UpdateZNFlags(tempByte);

    _a = tempByte;

    return HasPageCrossed(address, pointer) ? 6 : 5;
  }

  public cycle SBC_immediate(u8 operand)
  {
    var invertedValue = (u8)~operand;
    var tempInt = _a + invertedValue + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ invertedValue) & 0x80));
    UpdateZNFlags(tempByte);

    _a = tempByte;

    return 2;
  }
  public cycle SBC_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);
    var invertedValue = (u8)~value;
    var tempInt = _a + invertedValue + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ invertedValue) & 0x80));
    UpdateZNFlags(tempByte);

    _a = tempByte;

    return 3;
  }

  public cycle SBC_zero_page_x(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _x));
    var invertedValue = (u8)~value;
    var tempInt = _a + invertedValue + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ invertedValue) & 0x80));
    UpdateZNFlags(tempByte);

    _a = tempByte;

    return 4;
  }

  public cycle SBC_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);
    var invertedValue = (u8)~value;
    var tempInt = _a + invertedValue + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ invertedValue) & 0x80));
    UpdateZNFlags(tempByte);

    _a = tempByte;

    return 4;
  }

  public cycle SBC_absolute_x(u16 operand)
  {
    var value = _bus.ReadByte((u16)(operand + _x));
    var invertedValue = (u8)~value;
    var tempInt = _a + invertedValue + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ invertedValue) & 0x80));
    UpdateZNFlags(tempByte);

    _a = tempByte;

    return HasPageCrossed(operand + _x, operand) ? 5 : 4;
  }

  public cycle SBC_absolute_y(u16 operand)
  {
    var value = _bus.ReadByte((u16)(operand + _y));
    var invertedValue = (u8)~value;
    var tempInt = _a + invertedValue + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ invertedValue) & 0x80));
    UpdateZNFlags(tempByte);

    _a = tempByte;

    return HasPageCrossed(operand + _y, operand) ? 5 : 4;
  }

  public cycle SBC_indexed_indirect(u8 operand)
  {
    var lo = _bus.ReadByte((u8)(operand + _x));
    var hi = _bus.ReadByte((u8)(operand + _x + 1));
    var pointer = CombineBytesToWord(lo, hi);
    var value = _bus.ReadByte(pointer);

    var invertedValue = (u8)~value;
    var tempInt = _a + invertedValue + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ invertedValue) & 0x80));
    UpdateZNFlags(tempByte);

    _a = tempByte;

    return 6;
  }

  public cycle SBC_indirect_indexed(u8 operand)
  {
    var lo = _bus.ReadByte(operand);
    var hi = _bus.ReadByte((u8)(operand + 1));
    var pointer = CombineBytesToWord(lo, hi);
    var address = (u16)(pointer + _y);
    var value = _bus.ReadByte(address);

    var invertedValue = (u8)~value;
    var tempInt = _a + invertedValue + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (byte)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, NumberToBool((tempByte ^ _a) & (tempByte ^ invertedValue) & 0x80));
    UpdateZNFlags(tempByte);

    _a = tempByte;

    return HasPageCrossed(address, pointer) ? 6 : 5;
  }

  public cycle AND_immediate(u8 operand)
  {
    _a = (u8)(_a & operand);
    UpdateZNFlags(_a);

    return 2;
  }

  public cycle AND_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);
    _a = (u8)(_a & value);
    UpdateZNFlags(_a);

    return 3;
  }

  public cycle AND_zero_page_x(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _x));
    _a = (u8)(_a & value);
    UpdateZNFlags(_a);

    return 4;
  }

  public cycle AND_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);
    _a = (u8)(_a & value);
    UpdateZNFlags(_a);

    return 4;
  }

  public cycle AND_absolute_x(u16 operand)
  {
    var address = (u16)(operand + _x);
    var value = _bus.ReadByte(address);

    _a = (u8)(_a & value);
    UpdateZNFlags(_a);

    return HasPageCrossed(address, operand) ? 5 : 4;
  }

  public cycle AND_absolute_y(u16 operand)
  {
    var address = (u16)(operand + _y);
    var value = _bus.ReadByte(address);

    _a = (u8)(_a & value);
    UpdateZNFlags(_a);

    return HasPageCrossed(address, operand) ? 5 : 4;
  }

  public cycle AND_indexed_indirect(u8 operand)
  {
    var lo = _bus.ReadByte((u8)(operand + _x));
    var hi = _bus.ReadByte((u8)(operand + _x + 1));
    var pointer = CombineBytesToWord(lo, hi);

    var value = _bus.ReadByte(pointer);

    _a = (u8)(_a & value);
    UpdateZNFlags(_a);

    return 6;
  }

  public cycle AND_indirect_indexed(u8 operand)
  {
    var lo = _bus.ReadByte(operand);
    var hi = _bus.ReadByte((u8)(operand + 1));

    var pointer = CombineBytesToWord(lo, hi);
    var address = (u16)(pointer + _y);

    var value = _bus.ReadByte(address);

    _a = (u8)(_a & value);
    UpdateZNFlags(_a);

    return HasPageCrossed(address, pointer) ? 6 : 5;
  }

  public cycle ORA_immediate(u8 operand)
  {
    _a = (u8)(_a | operand);
    UpdateZNFlags(_a);

    return 2;
  }

  public cycle ORA_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);
    _a = (u8)(_a | value);
    UpdateZNFlags(_a);

    return 3;
  }

  public cycle ORA_zero_page_x(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _x));
    _a = (u8)(_a | value);
    UpdateZNFlags(_a);

    return 4;
  }

  public cycle ORA_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);
    _a = (u8)(_a | value);
    UpdateZNFlags(_a);

    return 4;
  }

  public cycle ORA_absolute_x(u16 operand)
  {
    var address = (u16)(operand + _x);
    var value = _bus.ReadByte(address);

    _a = (u8)(_a | value);
    UpdateZNFlags(_a);

    return HasPageCrossed(address, operand) ? 5 : 4;
  }

  public cycle ORA_absolute_y(u16 operand)
  {
    var address = (u16)(operand + _y);
    var value = _bus.ReadByte(address);

    _a = (u8)(_a | value);
    UpdateZNFlags(_a);

    return HasPageCrossed(address, operand) ? 5 : 4;
  }

  public cycle ORA_indexed_indirect(u8 operand)
  {
    var lo = _bus.ReadByte((u8)(operand + _x));
    var hi = _bus.ReadByte((u8)(operand + _x + 1));
    var pointer = CombineBytesToWord(lo, hi);

    var value = _bus.ReadByte(pointer);

    _a = (u8)(_a | value);
    UpdateZNFlags(_a);

    return 6;
  }

  public cycle ORA_indirect_indexed(u8 operand)
  {
    var lo = _bus.ReadByte(operand);
    var hi = _bus.ReadByte((u8)(operand + 1));

    var pointer = CombineBytesToWord(lo, hi);
    var address = (u16)(pointer + _y);

    var value = _bus.ReadByte(address);

    _a = (u8)(_a | value);
    UpdateZNFlags(_a);

    return HasPageCrossed(address, pointer) ? 6 : 5;
  }

  public cycle EOR_immediate(u8 operand)
  {
    _a = (u8)(_a ^ operand);
    UpdateZNFlags(_a);

    return 2;
  }

  public cycle EOR_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);
    _a = (u8)(_a ^ value);
    UpdateZNFlags(_a);

    return 3;
  }

  public cycle EOR_zero_page_x(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _x));
    _a = (u8)(_a ^ value);
    UpdateZNFlags(_a);

    return 4;
  }

  public cycle EOR_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);
    _a = (u8)(_a ^ value);
    UpdateZNFlags(_a);

    return 4;
  }

  public cycle EOR_absolute_x(u16 operand)
  {
    var address = (u16)(operand + _x);
    var value = _bus.ReadByte(address);

    _a = (u8)(_a ^ value);
    UpdateZNFlags(_a);

    return HasPageCrossed(address, operand) ? 5 : 4;
  }

  public cycle EOR_absolute_y(u16 operand)
  {
    var address = (u16)(operand + _y);
    var value = _bus.ReadByte(address);

    _a = (u8)(_a ^ value);
    UpdateZNFlags(_a);

    return HasPageCrossed(address, operand) ? 5 : 4;
  }

  public cycle EOR_indexed_indirect(u8 operand)
  {
    var lo = _bus.ReadByte((u8)(operand + _x));
    var hi = _bus.ReadByte((u8)(operand + _x + 1));
    var pointer = CombineBytesToWord(lo, hi);

    var value = _bus.ReadByte(pointer);

    _a = (u8)(_a ^ value);
    UpdateZNFlags(_a);

    return 6;
  }

  public cycle EOR_indirect_indexed(u8 operand)
  {
    var lo = _bus.ReadByte(operand);
    var hi = _bus.ReadByte((u8)(operand + 1));

    var pointer = CombineBytesToWord(lo, hi);
    var address = (u16)(pointer + _y);

    var value = _bus.ReadByte(address);

    _a = (u8)(_a ^ value);
    UpdateZNFlags(_a);

    return HasPageCrossed(address, pointer) ? 6 : 5;
  }

  public cycle CMP_immediate(u8 operand)
  {
    CompareRegisterAndValue(_a, operand);
    return 2;
  }

  public cycle CMP_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);
    CompareRegisterAndValue(_a, value);
    return 3;
  }

  public cycle CMP_zero_page_x(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _x));
    CompareRegisterAndValue(_a, value);
    return 4;
  }

  public cycle CMP_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);
    CompareRegisterAndValue(_a, value);
    return 4;
  }

  public cycle CMP_absolute_x(u16 operand)
  {
    var address = (u16)(operand + _x);
    var value = _bus.ReadByte(address);
    CompareRegisterAndValue(_a, value);
    return HasPageCrossed(address, operand) ? 5 : 4;
  }

  public cycle CMP_absolute_y(u16 operand)
  {
    var address = (u16)(operand + _y);
    var value = _bus.ReadByte(address);
    CompareRegisterAndValue(_a, value);
    return HasPageCrossed(address, operand) ? 5 : 4;
  }

  public cycle CMP_indexed_indirect(u8 operand)
  {
    var lo = _bus.ReadByte((u8)(operand + _x));
    var hi = _bus.ReadByte((u8)(operand + _x + 1));
    var pointer = CombineBytesToWord(lo, hi);
    var value = _bus.ReadByte(pointer);
    CompareRegisterAndValue(_a, value);
    return 6;
  }

  public cycle CMP_indirect_indexed(u8 operand)
  {
    var lo = _bus.ReadByte(operand);
    var hi = _bus.ReadByte((u8)(operand + 1));
    var pointer = CombineBytesToWord(lo, hi);
    var address = (u16)(pointer + _y);
    var value = _bus.ReadByte(address);
    CompareRegisterAndValue(_a, value);
    return HasPageCrossed(address, pointer) ? 6 : 5;
  }

  public cycle CPX_immediate(u8 operand)
  {
    CompareRegisterAndValue(_x, operand);
    return 2;
  }

  public cycle CPX_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);
    CompareRegisterAndValue(_x, value);
    return 3;
  }

  public cycle CPX_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);
    CompareRegisterAndValue(_x, value);
    return 4;
  }

  public cycle CPY_immediate(u8 operand)
  {
    CompareRegisterAndValue(_y, operand);
    return 2;
  }

  public cycle CPY_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);
    CompareRegisterAndValue(_y, value);
    return 3;
  }

  public cycle CPY_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);
    CompareRegisterAndValue(_y, value);
    return 4;
  }

  private void CompareRegisterAndValue(u8 reg, u8 value)
  {
    var diff = (u8)(reg - value);
    SetFlag(Status.Carry, reg >= value);
    UpdateZNFlags(diff);
  }

  public void Reset()
  {
    _stkp = (u8)(_stkp - 3);
    _pc = _bus.ReadWord(0xFFFC);
    _status = 0x00 | Status.Interrupt;
  }

  private void SetFlag(Status flag, bool active)
  {
    if (active) _status |= flag;
    else _status &= ~flag;
  }

  private bool HasFlag(Status flag)
  {
    return (_status & flag) > 0;
  }

  private void UpdateZNFlags(u8 value)
  {
    SetFlag(Status.Zero, value == 0);
    SetFlag(Status.Negative, (value & 0x80) != 0);
  }

  private static bool HasPageCrossed(int a, int b)
  {
    return (a >> 8) != (b >> 8);
  }

  private static bool NumberToBool<T>(T number) where T : INumber<T>
  {
    return number != T.Zero;
  }

  private static u16 CombineBytesToWord(u8 lo, u8 hi)
  {
    return (u16)(lo | hi << 8);
  }
}