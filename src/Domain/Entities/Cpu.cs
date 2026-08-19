using System.Numerics;
using mos6502.src.Domain.Enums;
using mos6502.src.Domain.Objects;

namespace mos6502.src.Domain.Entities;

using Status = mos6502.src.Domain.Enums.Status;

public class Cpu(Bus bus)
{

  public Bus Bus { get; } = bus;

  public Unassigned16Bits ProgramCounter { get; set; } = new(0xFFFC);
  public Unassigned16Bits StackPointer { get; set; } = new(0x00FD);
  public Unassigned8Bits Accumulator { get; set; } = new(0x00);
  public Unassigned8Bits XRegister { get; set; } = new(0x00);
  public Unassigned8Bits YRegister { get; set; } = new(0x00);
  public Status Flags { get; set; } = Status.Interrupt;

  public void AdvancePC(int bytes)
  {
    ProgramCounter = (Unassigned16Bits)(ProgramCounter + bytes);
  }

  public void ADC(AddressingMode mode, Unassigned16Bits operand)
  {
    var data = ReadOperand(mode, operand);
    var temp = Accumulator + data + (IsFlag(Status.Carry) ? 1 : 0);

    SetFlag(Status.Carry, temp > 0xFF);
    SetFlag(Status.Zero, (temp & 0xFF) == 0);
    SetFlag(Status.Overflow, ToBool((temp ^ Accumulator) & (temp ^ data) & 0b1000_0000));
    SetFlag(Status.Negative, ToBool(temp & 0b1000_0000));

    Accumulator = (Unassigned8Bits)temp;
  }

  internal Unassigned16Bits ReadOperand(AddressingMode mode, Unassigned16Bits operand)
  {
    return mode switch
    {
      AddressingMode.Immediate => operand,
      AddressingMode.Accumulator => Accumulator,
      AddressingMode.Relative => Bus.Read((Unassigned16Bits)(ProgramCounter + operand)),

      AddressingMode.ZeroPage => Bus.Read((Unassigned16Bits)(operand & 0xFF)),
      AddressingMode.ZeroPageX => Bus.Read((Unassigned16Bits)((operand + XRegister) & 0xFF)),
      AddressingMode.ZeroPageY => Bus.Read((Unassigned16Bits)((operand + YRegister) & 0xFF)),

      AddressingMode.Absolute => Bus.Read(operand),
      AddressingMode.AbsoluteX => Bus.Read((Unassigned16Bits)(operand + XRegister)),
      AddressingMode.AbsoluteY => Bus.Read((Unassigned16Bits)(operand + YRegister)),

      AddressingMode.IndexedIndirect => ReadIndexedIndirect(operand),
      AddressingMode.IndirectIndexed => ReadIndirectIndexed(operand),

      _ => throw new Exception("Addressing mode not implemented"),
    };
  }

  private Unassigned16Bits ReadIndexedIndirect(Unassigned16Bits operand)
  {
    var pointer = (Unassigned16Bits)((operand + XRegister) & 0xFF);
    var lo = Bus.Read(pointer);
    var hi = Bus.Read((Unassigned16Bits)((pointer + 1) & 0xFF));
    return Bus.Read((Unassigned16Bits)(lo.Value | (hi.Value << 8)));
  }

  private Unassigned16Bits ReadIndirectIndexed(Unassigned16Bits operand)
  {
    var pointer = (Unassigned16Bits)(operand & 0xFF);
    var lo = Bus.Read(pointer);
    var hi = Bus.Read((Unassigned16Bits)((pointer + 1) & 0xFF));
    return Bus.Read((Unassigned16Bits)((lo.Value | (hi.Value << 8)) + YRegister));
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