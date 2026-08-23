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

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _a = value;

    return 2;
  }

  public cycle LDA_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _a = value;

    return 3;
  }

  public cycle LDA_zero_page_x(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _x));

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _a = value;

    return 4;
  }

  public cycle LDA_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _a = value;

    return 4;
  }

  public cycle LDA_absolute_x(u16 operand)
  {
    var temp = operand + _x;
    var value = _bus.ReadByte((u16)(temp));

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _a = value;

    return HasPageCrossed(operand, temp) ? 5 : 4;
  }

  public cycle LDA_absolute_y(u16 operand)
  {
    var temp = operand + _y;
    var value = _bus.ReadByte((u16)(temp));

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _a = value;

    return HasPageCrossed(operand, temp) ? 5 : 4;
  }

  public cycle LDA_indexed_indirect(u8 operand)
  {
    var lo = _bus.ReadByte((u8)(operand + _x));
    var hi = _bus.ReadByte((u8)(operand + _x + 1));
    var pointer = (u16)(lo | hi << 8);

    var value = _bus.ReadByte(pointer);

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _a = value;

    return 6;
  }

  public cycle LDA_indirect_indexed(u8 operand)
  {
    var lo = _bus.ReadByte(operand);
    var hi = _bus.ReadByte((u8)(operand + 1));
    var pointer = (u16)(lo | hi << 8);
    var address = (u16)(pointer + _y);
    var value = _bus.ReadByte(address);

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _a = value;

    return HasPageCrossed(pointer, address) ? 6 : 5;
  }

  public cycle LDX_immediate(u8 operand)
  {
    var value = operand;

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _x = value;

    return 2;
  }

  public cycle LDX_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _x = value;

    return 3;
  }

  public cycle LDX_zero_page_y(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _y));

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _x = value;

    return 4;
  }

  public cycle LDX_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _x = value;

    return 4;
  }

  public cycle LDX_absolute_y(u16 operand)
  {
    var temp = operand + _y;
    var value = _bus.ReadByte((u16)temp);

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _x = value;

    return HasPageCrossed(operand, temp) ? 5 : 4;
  }

  public cycle LDY_immediate(u8 operand)
  {
    var value = operand;

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _y = value;

    return 2;
  }

  public cycle LDY_zero_page(u8 operand)
  {
    var value = _bus.ReadByte(operand);

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _y = value;

    return 3;
  }

  public cycle LDY_zero_page_x(u8 operand)
  {
    var value = _bus.ReadByte((u8)(operand + _x));

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _y = value;

    return 4;
  }

  public cycle LDY_absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

    _y = value;

    return 4;
  }

  public cycle LDY_absolute_x(u16 operand)
  {
    var temp = operand + _x;
    var value = _bus.ReadByte((u16)temp);

    SetFlag(Status.Negative, (value & 0x80) != 0);
    SetFlag(Status.Zero, value == 0x00);

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
    var address = (u16)(lo | hi << 8);

    _bus.WriteByte(address, _a);

    return 6;
  }

  public cycle STA_indirect_indexed(u8 operand)
  {
    var lo = _bus.ReadByte(operand);
    var hi = _bus.ReadByte((u8)(operand + 1));
    var pointer = (u16)(lo | hi << 8);
    var address = (u16)(pointer + _y);

    _bus.WriteByte(address, _a);


    return HasPageCrossed(pointer, address) ? 6 : 5;
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

  private static bool HasPageCrossed(int a, int b)
  {
    return (a >> 8) != (b >> 8);
  }
}