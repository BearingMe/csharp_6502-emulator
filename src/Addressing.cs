namespace mos6502;

public class Addressing(Bus bus, Emulator cpu)
{
  private readonly Bus _bus = bus;
  private readonly Emulator _cpu = cpu;

  public AddressingResult<u8> Immediate(u8 operand)
  {
    return new(operand, 0);
  }

  public AddressingResult<u8> ZeroPage(u8 operand)
  {
    var value = _bus.ReadByte(operand);
    return new(value, 0);
  }

  public AddressingResult<u8> ZeroPageX(u8 operand)
  {
    var address = (u8)(operand + _cpu.X);
    var value = _bus.ReadByte(address);
    return new(value, 0);
  }

  public AddressingResult<u8> ZeroPageY(u8 operand)
  {
    var address = (u8)(operand + _cpu.Y);
    var value = _bus.ReadByte(address);
    return new(value, 0);
  }

  public AddressingResult<u8> Absolute(u16 operand)
  {
    var value = _bus.ReadByte(operand);
    return new(value, 0);
  }

  public AddressingResult<u8> AbsoluteX(u16 operand)
  {
    var address = (u16)(operand + _cpu.X);
    var value = _bus.ReadByte(address);
    var extraCycle = HasPageCrossed(operand, address) ? 1 : 0;
    return new(value, extraCycle);
  }

  public AddressingResult<u8> AbsoluteY(u16 operand)
  {
    var address = (u16)(operand + _cpu.Y);
    var value = _bus.ReadByte(address);
    var extraCycle = HasPageCrossed(operand, address) ? 1 : 0;
    return new(value, extraCycle);
  }

  public AddressingResult<u8> IndexedIndirect(u8 operand)
  {
    var lo = _bus.ReadByte((u8)(operand + _cpu.X));
    var hi = _bus.ReadByte((u8)(operand + _cpu.X + 1));
    var pointer = CombineBytesToWord(lo, hi);
    var value = _bus.ReadByte(pointer);
    return new(value, 0);
  }

  public AddressingResult<u8> IndirectIndexed(u8 operand)
  {
    var lo = _bus.ReadByte(operand);
    var hi = _bus.ReadByte((u8)(operand + 1));
    var pointer = CombineBytesToWord(lo, hi);
    var address = (u16)(pointer + _cpu.Y);
    var value = _bus.ReadByte(address);
    var extraCycle = HasPageCrossed(pointer, address) ? 1 : 0;
    return new(value, extraCycle);
  }

  private static bool HasPageCrossed(int a, int b)
  {
    return (a >> 8) != (b >> 8);
  }

  private static u16 CombineBytesToWord(u8 lo, u8 hi)
  {
    return (u16)(lo | hi << 8);
  }
}
