namespace mos6502.src.Application;

using mos6502.src.Domain.Entities;
using mos6502.src.Domain.Enums;
using mos6502.src.Domain.Objects;

public class Emulator
{
  public Bus Bus { get; }
  public Cpu Cpu { get; }

  public Emulator()
  {
    Bus = new Bus();
    Cpu = new Cpu(Bus);
  }

  public void LoadProgram(byte[] program, ushort startAddress)
  {
    for (int i = 0; i < program.Length; i++)
    {
      Bus.Write(
        (Unassigned16Bits)(startAddress + i),
        (Unassigned8Bits)program[i]
      );
    }
  }

  public void Reset()
  {
    Cpu.ProgramCounter = new Unassigned16Bits(0xFFFC);
    Cpu.StackPointer = new Unassigned16Bits(0x00FD);
    Cpu.Accumulator = new Unassigned8Bits(0x00);
    Cpu.XRegister = new Unassigned8Bits(0x00);
    Cpu.YRegister = new Unassigned8Bits(0x00);
    Cpu.Flags = Status.Interrupt;
  }

  public void Step()
  {
    var opcode = Bus.Read(Cpu.ProgramCounter);
    var (mode, length) = DecodeInstruction(opcode.Value);
    var operand = FetchOperand(length);

    ExecuteInstruction(opcode.Value, mode, operand);
    Cpu.AdvancePC(length);
  }

  private static (AddressingMode mode, int length) DecodeInstruction(byte opcode)
  {
    return opcode switch
    {
      // ADC (Add with Carry)
      0x69 => (AddressingMode.Immediate, 2),
      0x65 => (AddressingMode.ZeroPage, 2),
      0x75 => (AddressingMode.ZeroPageX, 2),
      0x6D => (AddressingMode.Absolute, 3),
      0x7D => (AddressingMode.AbsoluteX, 3),
      0x79 => (AddressingMode.AbsoluteY, 3),
      0x61 => (AddressingMode.IndexedIndirect, 2),
      0x71 => (AddressingMode.IndirectIndexed, 2),

      _ => throw new Exception($"Unknown opcode: {opcode:X2}"),
    };
  }

  private Unassigned16Bits FetchOperand(int length)
  {
    if (length == 1)
      return new Unassigned16Bits(0x0000);

    var lo = Bus.Read((Unassigned16Bits)(Cpu.ProgramCounter + 1));
    if (length == 2)
      return lo;

    var hi = Bus.Read((Unassigned16Bits)(Cpu.ProgramCounter + 2));
    return (Unassigned16Bits)(lo.Value | (hi.Value << 8));
  }

  private void ExecuteInstruction(byte opcode, AddressingMode mode, Unassigned16Bits operand)
  {
    switch (opcode)
    {
      // ADC (Add with Carry)
      case 0x69:
      case 0x65:
      case 0x75:
      case 0x6D:
      case 0x7D:
      case 0x79:
      case 0x61:
      case 0x71:
        Cpu.ADC(mode, operand);
        break;

      default:
        throw new Exception($"Unknown opcode: {opcode:X2}");
    }
  }
}
