using mos6502.src.Domain.Objects;
using mos6502.src.Domain.Enums;
using Microsoft.VisualBasic;

namespace mos6502.src.Domain.Entities;

public class Instructions(Cpu cpu)
{
  private readonly Cpu _cpu = cpu;

  public InstructionResult LDA(u8 value)
  {
    UpdateZNFlags(value);
    _cpu.A = value;

    return new(2);
  }

  public InstructionResult LDX(u8 value)
  {
    UpdateZNFlags(value);
    _cpu.X = value;

    return new(2);
  }

  public InstructionResult LDY(u8 value)
  {
    UpdateZNFlags(value);
    _cpu.Y = value;

    return new(2);
  }

  public InstructionResult STA(u16 address)
  {
    _cpu.WriteByte(address, _cpu.A);

    return new(2);
  }

  public InstructionResult STX(u16 address)
  {
    _cpu.WriteByte(address, _cpu.X);

    return new(2);
  }

  public InstructionResult STY(u16 address)
  {
    _cpu.WriteByte(address, _cpu.Y);

    return new(2);
  }

  public InstructionResult TAX()
  {
    _cpu.X = _cpu.A;
    UpdateZNFlags(_cpu.X);

    return new(2);
  }

  public InstructionResult TAY()
  {
    _cpu.Y = _cpu.A;
    UpdateZNFlags(_cpu.Y);

    return new(2);
  }

  public InstructionResult TSX()
  {
    _cpu.X = _cpu.StackPointer;
    UpdateZNFlags(_cpu.X);

    return new(2);
  }

  public InstructionResult TXA()
  {
    _cpu.A = _cpu.X;
    UpdateZNFlags(_cpu.A);

    return new(2);
  }

  public InstructionResult TXS()
  {
    _cpu.StackPointer = _cpu.X;

    return new(2);
  }

  public InstructionResult TYA()
  {
    _cpu.A = _cpu.Y;
    UpdateZNFlags(_cpu.A);

    return new(2);
  }

  public InstructionResult INX()
  {
    _cpu.X = (u8)(_cpu.X + 1);
    UpdateZNFlags(_cpu.X);

    return new(2);
  }

  public InstructionResult INY()
  {
    _cpu.Y = (u8)(_cpu.Y + 1);
    UpdateZNFlags(_cpu.Y);

    return new(2);
  }

  public InstructionResult ADC(u8 value)
  {
    var tempInt = _cpu.A + value + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (u8)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, ((tempByte ^ _cpu.A) & (tempByte ^ value) & 0x80) != 0);
    UpdateZNFlags(tempByte);

    _cpu.A = tempByte;

    return new(2);
  }

  public InstructionResult SBC(u8 value)
  {
    var invertedValue = (u8)~value;
    var tempInt = _cpu.A + invertedValue + (HasFlag(Status.Carry) ? 1 : 0);
    var tempByte = (u8)tempInt;

    SetFlag(Status.Carry, tempInt > 0xFF);
    SetFlag(Status.Overflow, ((tempByte ^ _cpu.A) & (tempByte ^ invertedValue) & 0x80) != 0);
    UpdateZNFlags(tempByte);

    _cpu.A = tempByte;

    return new(2);
  }

  public InstructionResult AND(u8 value)
  {
    _cpu.A = (u8)(_cpu.A & value);
    UpdateZNFlags(_cpu.A);

    return new(2);
  }

  public InstructionResult ORA(u8 value)
  {
    _cpu.A = (u8)(_cpu.A | value);
    UpdateZNFlags(_cpu.A);

    return new(2);
  }

  public InstructionResult EOR(u8 value)
  {
    _cpu.A = (u8)(_cpu.A ^ value);
    UpdateZNFlags(_cpu.A);

    return new(2);
  }

  public InstructionResult BIT(u8 value)
  {
    var result = _cpu.A & value;

    SetFlag(Status.Zero, result == 0);
    SetFlag(Status.Overflow, (value & 0x40) != 0);
    SetFlag(Status.Negative, (value & 0x80) != 0);

    return new(2);
  }

  public InstructionResult CMP(u8 value)
  {
    CompareRegisterAndValue(_cpu.A, value);

    return new(2);
  }

  public InstructionResult CPX(u8 value)
  {
    CompareRegisterAndValue(_cpu.X, value);

    return new(2);
  }

  public InstructionResult CPY(u8 value)
  {
    CompareRegisterAndValue(_cpu.Y, value);

    return new(2);
  }

  public InstructionResult BCC(i8 offset) => Branch(!HasFlag(Status.Carry), offset);

  public InstructionResult BCS(i8 offset) => Branch(HasFlag(Status.Carry), offset);

  public InstructionResult BEQ(i8 offset) => Branch(HasFlag(Status.Zero), offset);

  public InstructionResult BNE(i8 offset) => Branch(!HasFlag(Status.Zero), offset);

  public InstructionResult BMI(i8 offset) => Branch(HasFlag(Status.Negative), offset);

  public InstructionResult BPL(i8 offset) => Branch(!HasFlag(Status.Negative), offset);

  public InstructionResult BVC(i8 offset) => Branch(!HasFlag(Status.Overflow), offset);

  public InstructionResult BVS(i8 offset) => Branch(HasFlag(Status.Overflow), offset);

  public InstructionResult DEX()
  {
    var result = (byte)(_cpu.X - 1);

    UpdateZNFlags(result);

    _cpu.X = result;

    return new(2);
  }

  public InstructionResult DEY()
  {
    var result = (byte)(_cpu.Y - 1);

    UpdateZNFlags(result);

    _cpu.Y = result;

    return new(2);
  }

  public InstructionResult ASL()
  {
    var result = (u8)(_cpu.A << 1);

    SetFlag(Status.Carry, (_cpu.A & 0x80) != 0);
    UpdateZNFlags(result);
    _cpu.A = result;

    return new(2);
  }

  public InstructionResult ASL(u16 address)
  {
    var value = _cpu.ReadByte(address);
    var result = (u8)(value << 1);

    SetFlag(Status.Carry, (value & 0x80) != 0);
    UpdateZNFlags(result);
    _cpu.WriteByte(address, result);

    return new(4);
  }

  public InstructionResult LSR()
  {
    var result = (u8)(_cpu.A >> 1);

    SetFlag(Status.Carry, (_cpu.A & 0x01) != 0);
    UpdateZNFlags(result);
    _cpu.A = result;

    return new(2);
  }

  public InstructionResult LSR(u16 address)
  {
    var value = _cpu.ReadByte(address);
    var result = (u8)(value >> 1);

    SetFlag(Status.Carry, (value & 0x01) != 0);
    UpdateZNFlags(result);
    _cpu.WriteByte(address, result);

    return new(4);
  }

  public InstructionResult ROL()
  {
    var temp = (_cpu.A << 1) | (HasFlag(Status.Carry) ? 1 : 0);
    var result = (u8)temp;

    SetFlag(Status.Carry, (_cpu.A & 0x80) != 0);
    UpdateZNFlags(result);

    _cpu.A = result;

    return new(2);
  }

  public InstructionResult ROL(u16 address)
  {
    var value = _cpu.ReadByte(address);

    var temp = (value << 1) | (HasFlag(Status.Carry) ? 1 : 0);
    var result = (u8)temp;

    SetFlag(Status.Carry, (value & 0x80) != 0);
    UpdateZNFlags(result);

    _cpu.WriteByte(address, result);

    return new(4);
  }

  public InstructionResult ROR()
  {
    var temp = (_cpu.A >> 1) | (HasFlag(Status.Carry) ? 0x80 : 0);
    var result = (u8)temp;

    SetFlag(Status.Carry, (_cpu.A & 0x01) != 0);
    UpdateZNFlags(result);

    _cpu.A = result;

    return new(2);
  }

  public InstructionResult ROR(u16 address)
  {
    var value = _cpu.ReadByte(address);

    var temp = (value >> 1) | (HasFlag(Status.Carry) ? 0x80 : 0);
    var result = (u8)temp;

    SetFlag(Status.Carry, (value & 0x01) != 0);
    UpdateZNFlags(result);

    _cpu.WriteByte(address, result);

    return new(4);
  }

  // private
  private InstructionResult Branch(bool condition, i8 offset)
  {
    if (condition)
    {
      var oldPc = _cpu.PC;
      var newPc = (u16)(_cpu.PC + offset);

      _cpu.PC = newPc;

      var extraCycle = Addressing.HasPageCrossed(oldPc, newPc) ? 1 : 0;
      return new(3 + extraCycle);
    }

    return new(2);
  }

  private void CompareRegisterAndValue(u8 reg, u8 value)
  {
    var diff = (u8)(reg - value);
    SetFlag(Status.Carry, reg >= value);
    UpdateZNFlags(diff);
  }

  private void SetFlag(Status flag, bool active)
  {
    if (active) _cpu.Status |= flag;
    else _cpu.Status &= ~flag;
  }

  private bool HasFlag(Status flag)
  {
    return (_cpu.Status & flag) > 0;
  }

  private void UpdateZNFlags(u8 value)
  {
    SetFlag(Status.Zero, value == 0);
    SetFlag(Status.Negative, (value & 0x80) != 0);
  }
}
