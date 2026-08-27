using mos6502.src.Domain.Enums;

namespace mos6502.src.Domain.Entities;

public class Cpu
{
  // registers
  public u8 A { get; internal set; }
  public u8 X { get; internal set; }
  public u8 Y { get; internal set; }
  public u8 StackPointer { get; internal set; }
  public u16 PC { get; internal set; }
  public Status Status { get; internal set; }

  // memory
  public u8[] Ram { get; internal set; } = new u8[0x10000];

  // addons
  public Addressing Addressing { get; internal set; }
  public Instructions Instructions { get; internal set; }

  public Cpu()
  {
    A = 0x00;
    X = 0x00;
    Y = 0x00;
    StackPointer = 0xFD;
    PC = ReadWord(0xFFFC);
    Status = 0x00 | Status.Interrupt;

    Addressing = new(this);
    Instructions = new(this);
  }

  public void WriteByte(u16 address, u8 data) => Ram[address] = data;

  public u8 ReadByte(u16 address) => Ram[address];

  public u16 ReadWord(u16 address)
  {
    var lo = Ram[address];
    var hi = Ram[(u16)(address + 1)];

    return (u16)(lo | hi << 8);
  }

  public u8 FetchByte()
  {
    var value = ReadByte(PC);
    PC++;
    return value;
  }

  public u16 FetchWord()
  {
    var value = ReadWord(PC);
    PC += 2;
    return value;
  }

  public void StackPush(u8 value)
  {
    WriteByte((u16)(0x0100 | StackPointer), value);
    StackPointer = (u8)(StackPointer - 1);
  }

  public u8 StackPop()
  {
    StackPointer = (u8)(StackPointer + 1);
    return ReadByte((u16)(0x0100 | StackPointer));
  }
}