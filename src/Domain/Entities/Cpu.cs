using System.Numerics;
using mos6502.src.Domain.Enums;

namespace mos6502.src.Domain.Entities;

public class Cpu(Bus bus)
{
  public Bus Bus { get; } = bus;

  public u16 ProgramCounter { get; set; } = 0xFFFC;
  public u16 StackPointer { get; set; } = 0x00FD;
  public u8 Accumulator { get; set; } = 0x00;
  public u8 XRegister { get; set; } = 0x00;
  public u8 YRegister { get; set; } = 0x00;
  public Status Flags { get; set; } = Status.Interrupt;

  public void AdvancePC(int bytes)
  {
    ProgramCounter = (u16)(ProgramCounter + bytes);
  }

  public void ADC(AddressingMode mode, u16 operand)
  {
    var data = ReadOperand(mode, operand);
    var temp = Accumulator + data + (IsFlag(Status.Carry) ? 1 : 0);

    SetFlag(Status.Carry, temp > 0xFF);
    SetFlag(Status.Zero, (temp & 0xFF) == 0);
    SetFlag(Status.Overflow, ToBool((temp ^ Accumulator) & (temp ^ data) & 0b1000_0000));
    SetFlag(Status.Negative, ToBool(temp & 0b1000_0000));

    Accumulator = (u8)temp;
  }

  public void AND(AddressingMode mode, u16 operand)
  {
    var data = ReadOperand(mode, operand);
    var temp = Accumulator & data;

    SetFlag(Status.Zero, temp == 0);
    SetFlag(Status.Negative, ToBool(temp & 0b1000_0000));

    Accumulator = (u8)temp;
  }

  public void ASL(AddressingMode mode, u16 operand)
  {
    var data = ReadOperand(mode, operand);
    var temp = data << 1;

    SetFlag(Status.Carry, ToBool(data & 0b1000_0000));
    SetFlag(Status.Zero, (temp & 0xFF) == 0);
    SetFlag(Status.Negative, ToBool(temp & 0b1000_0000));

    if (mode == AddressingMode.Accumulator)
      Accumulator = (u8)(temp & 0xFF);
    else
      Bus.Write(operand, (u8)(temp & 0xFF));
  }

  internal u16 ReadOperand(AddressingMode mode, u16 operand)
  {
    return mode switch
    {
      AddressingMode.Immediate => operand,
      AddressingMode.Accumulator => Accumulator,
      AddressingMode.Relative => Bus.Read((u16)(ProgramCounter + operand)),

      AddressingMode.ZeroPage => Bus.Read((u16)(operand & 0xFF)),
      AddressingMode.ZeroPageX => Bus.Read((u16)((operand + XRegister) & 0xFF)),
      AddressingMode.ZeroPageY => Bus.Read((u16)((operand + YRegister) & 0xFF)),

      AddressingMode.Absolute => Bus.Read(operand),
      AddressingMode.AbsoluteX => Bus.Read((u16)(operand + XRegister)),
      AddressingMode.AbsoluteY => Bus.Read((u16)(operand + YRegister)),

      AddressingMode.IndexedIndirect => ReadIndexedIndirect(operand),
      AddressingMode.IndirectIndexed => ReadIndirectIndexed(operand),

      _ => throw new Exception("Addressing mode not implemented"),
    };
  }

  private u16 ReadIndexedIndirect(u16 operand)
  {
    var pointer = (u16)((operand + XRegister) & 0xFF);
    var lo = Bus.Read(pointer);
    var hi = Bus.Read((u16)((pointer + 1) & 0xFF));
    return Bus.Read((u16)(lo | (hi << 8)));
  }

  private u16 ReadIndirectIndexed(u16 operand)
  {
    var pointer = (u16)(operand & 0xFF);
    var lo = Bus.Read(pointer);
    var hi = Bus.Read((u16)((pointer + 1) & 0xFF));
    return Bus.Read((u16)((lo | (hi << 8)) + YRegister));
  }

  private void SetFlag(Status flag, bool active)
  {
    if (active) Flags |= flag;
    else Flags &= ~flag;
  }

  private bool IsFlag(Status flag)
  {
    return (Flags & flag) > 0;
  }

  static bool ToBool<T>(T value) where T : INumber<T>
  {
    return value > T.One;
  }
}