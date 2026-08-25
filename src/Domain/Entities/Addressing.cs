using mos6502.src.Domain.Objects;

namespace mos6502.src.Domain.Entities;

public class Addressing(Cpu cpu)
{
  private readonly Cpu _cpu = cpu;

  public AddressingResult<u8> Immediate(u8 operand)
  {
    return new(operand, 0);
  }

  public AddressingResult<u16> ZeroPage(u8 operand)
  {
    return new(operand, 1);
  }

  public AddressingResult<u16> ZeroPageX(u8 operand)
  {
    var address = (u8)(operand + _cpu.X);

    return new(address, 2);
  }

  public AddressingResult<u16> ZeroPageY(u8 operand)
  {
    var address = (u8)(operand + _cpu.Y);

    return new(address, 2);
  }

  public AddressingResult<u16> Absolute(u16 operand)
  {
    return new(operand, 2);
  }

  public AddressingResult<u16> AbsoluteX(u16 operand)
  {
    var address = (u16)(operand + _cpu.X);
    var extraCycle = HasPageCrossed(operand, address) ? 1 : 0;

    return new(address, 2 + extraCycle);
  }

  public AddressingResult<u16> AbsoluteY(u16 operand)
  {
    var address = (u16)(operand + _cpu.Y);
    var extraCycle = HasPageCrossed(operand, address) ? 1 : 0;

    return new(address, 2 + extraCycle);
  }

  public AddressingResult<u16> IndexedIndirect(u8 operand)
  {
    var lo = _cpu.ReadByte((u8)(operand + _cpu.X));
    var hi = _cpu.ReadByte((u8)(operand + _cpu.X + 1));

    var address = CombineBytesToWord(lo, hi);

    return new(address, 4);
  }

  public AddressingResult<u16> IndirectIndexed(u8 operand)
  {
    var lo = _cpu.ReadByte(operand);
    var hi = _cpu.ReadByte((u8)(operand + 1));

    var pointer = CombineBytesToWord(lo, hi);
    var address = (u16)(pointer + _cpu.Y);
    var extraCycle = HasPageCrossed(pointer, address) ? 1 : 0;

    return new(address, 3 + extraCycle);
  }

  public AddressingResult<i8> Relative(u8 operand)
  {
    return new((i8)operand, 0);
  }

  internal static bool HasPageCrossed(int a, int b)
  {
    return (a >> 8) != (b >> 8);
  }

  private static u16 CombineBytesToWord(u8 lo, u8 hi)
  {
    return (u16)(lo | hi << 8);
  }
}
