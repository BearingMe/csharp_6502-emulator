using System.Numerics;
using mos6502.src.Domain.Enums;

namespace mos6502.src.Domain.Entities;

public class Cpu(Bus bus, Registers regs)
{
  public void AdvancePC(int bytes)
  {
    regs.PC = (u16)(regs.PC + bytes);
  }

  public void ADC(AddressingMode mode, u16 operand)
  {
    var data = ReadOperand(mode, operand);
    var temp = regs.Acc + data + (regs.IsFlag(Status.Carry) ? 1 : 0);

    regs.SetFlag(Status.Carry, temp > 0xFF);
    regs.SetFlag(Status.Zero, (temp & 0xFF) == 0);
    regs.SetFlag(Status.Overflow, ToBool((temp ^ regs.Acc) & (temp ^ data) & 0b1000_0000));
    regs.SetFlag(Status.Negative, ToBool(temp & 0b1000_0000));

    regs.Acc = (u8)temp;
  }

  public void AND(AddressingMode mode, u16 operand)
  {
    var data = ReadOperand(mode, operand);
    var temp = regs.Acc & data;

    regs.SetFlag(Status.Zero, temp == 0);
    regs.SetFlag(Status.Negative, ToBool(temp & 0b1000_0000));

    regs.Acc = (u8)temp;
  }

  public void ASL(AddressingMode mode, u16 operand)
  {
    var data = ReadOperand(mode, operand);
    var temp = data << 1;

    regs.SetFlag(Status.Carry, ToBool(data & 0b1000_0000));
    regs.SetFlag(Status.Zero, (temp & 0xFF) == 0);
    regs.SetFlag(Status.Negative, ToBool(temp & 0b1000_0000));

    if (mode == AddressingMode.Accumulator)
      regs.Acc = (u8)(temp & 0xFF);
    else
      bus.Write(operand, (u8)(temp & 0xFF));
  }

  internal u16 ReadOperand(AddressingMode mode, u16 operand)
  {
    return mode switch
    {
      AddressingMode.Immediate => operand,
      AddressingMode.Accumulator => regs.Acc,
      AddressingMode.Relative => bus.Read((u16)(regs.PC + operand)),

      AddressingMode.ZeroPage => bus.Read((u16)(operand & 0xFF)),
      AddressingMode.ZeroPageX => bus.Read((u16)((operand + regs.X) & 0xFF)),
      AddressingMode.ZeroPageY => bus.Read((u16)((operand + regs.Y) & 0xFF)),

      AddressingMode.Absolute => bus.Read(operand),
      AddressingMode.AbsoluteX => bus.Read((u16)(operand + regs.X)),
      AddressingMode.AbsoluteY => bus.Read((u16)(operand + regs.Y)),

      AddressingMode.IndexedIndirect => ReadIndexedIndirect(operand),
      AddressingMode.IndirectIndexed => ReadIndirectIndexed(operand),

      _ => throw new Exception("Addressing mode not implemented"),
    };
  }

  private u16 ReadIndexedIndirect(u16 operand)
  {
    var pointer = (u16)((operand + regs.X) & 0xFF);
    var lo = bus.Read(pointer);
    var hi = bus.Read((u16)((pointer + 1) & 0xFF));
    return bus.Read((u16)(lo | (hi << 8)));
  }

  private u16 ReadIndirectIndexed(u16 operand)
  {
    var pointer = (u16)(operand & 0xFF);
    var lo = bus.Read(pointer);
    var hi = bus.Read((u16)((pointer + 1) & 0xFF));
    return bus.Read((u16)((lo | (hi << 8)) + regs.Y));
  }

  static bool ToBool<T>(T value) where T : INumber<T>
  {
    return value > T.One;
  }
}