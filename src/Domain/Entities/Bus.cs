namespace Mos6502.Domain.Entities;

public class Bus
{
  public u8[] Ram { get; } = new u8[0x10000];

  public u8 ReadByte(u16 address) => Ram[address];

  public void WriteByte(u16 address, u8 data) => Ram[address] = data;

  public u16 ReadWord(u16 address)
  {
    var lo = Ram[address];
    var hi = Ram[(u16)(address + 1)];

    return (u16)(lo | hi << 8);
  }
}
