namespace mos6502.src.Domain.Entities;

public class Bus
{
  public u8[] Ram { get; set; } = new u8[0x10000]; // 64kb

  public void Write(u16 address, u8 data)
  {
    Ram[address] = data;
  }

  public u8 Read(u16 address)
  {
    return Ram[address];
  }

  public u16 Read16Bits(u16 address)
  {
    var lo = Read(address);
    var hi = Read((u16)(address + 1));

    return (u16)(lo | hi << 8);
  }
}