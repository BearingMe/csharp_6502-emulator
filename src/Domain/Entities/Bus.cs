using mos6502.src.Domain.Objects;

namespace mos6502.src.Domain.Entities;

public class Bus
{
  public const int RamSize = 0x10000; // 64kb
  public Unassigned8Bits[] Ram { get; set; } = new Unassigned8Bits[RamSize];

  public void Write(Unassigned16Bits address, Unassigned8Bits data)
  {
    Ram[address.Value] = data;
  }

  public Unassigned8Bits Read(Unassigned16Bits address)
  {
    return Ram[address.Value];
  }

  public Unassigned16Bits Read16Bits(Unassigned16Bits address)
  {
    var lo = Read(address);
    var hi = Read((Unassigned16Bits)(address + 1));

    return (Unassigned16Bits)(lo.Value | hi.Value << 8);
  }
}