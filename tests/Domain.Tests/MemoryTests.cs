using mos6502.src.Domain.Entities;
using mos6502.src.Domain.Objects;

namespace mos6502.Domain.Tests;

public class MemoryTests
{
  private readonly Bus bus = new();

  [Fact]
  public void WriteAndRead_PreservesValue()
  {
    bus.Write((Unassigned16Bits)0x1234, (Unassigned8Bits)0xAB);

    Assert.Equal(0xAB, bus.Read((Unassigned16Bits)0x1234).Value);
  }

  [Fact]
  public void Read16Bits_IsLittleEndian()
  {
    bus.Write((Unassigned16Bits)0x1234, (Unassigned8Bits)0x34);
    bus.Write((Unassigned16Bits)0x1235, (Unassigned8Bits)0x12);

    Assert.Equal(0x1234, bus.Read16Bits((Unassigned16Bits)0x1234).Value);
  }

  [Fact]
  public void Read16Bits_WrapsAtEndOfMemory()
  {
    bus.Write((Unassigned16Bits)0xFFFF, (Unassigned8Bits)0x34);
    bus.Write((Unassigned16Bits)0x0000, (Unassigned8Bits)0x12);

    Assert.Equal(0x1234, bus.Read16Bits((Unassigned16Bits)0xFFFF).Value);
  }

  [Fact]
  public void WriteAtEdges_PreservesValues()
  {
    bus.Write((Unassigned16Bits)0x0000, (Unassigned8Bits)0xAA);
    bus.Write((Unassigned16Bits)0xFFFF, (Unassigned8Bits)0xBB);

    Assert.Equal(0xAA, bus.Read((Unassigned16Bits)0x0000).Value);
    Assert.Equal(0xBB, bus.Read((Unassigned16Bits)0xFFFF).Value);
  }
}