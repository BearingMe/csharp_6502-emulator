using mos6502.src.Domain.Entities;

namespace mos6502.Domain.Tests;

public class MemoryTests
{
  private readonly Bus bus = new();

  [Fact]
  public void WriteAndRead_PreservesValue()
  {
    bus.Write(0x1234, 0xAB);

    Assert.Equal(0xAB, bus.Read(0x1234));
  }

  [Fact]
  public void Read16Bits_IsLittleEndian()
  {
    bus.Write(0x1234, 0x34);
    bus.Write(0x1235, 0x12);

    Assert.Equal(0x1234, bus.Read16Bits(0x1234));
  }

  [Fact]
  public void Read16Bits_WrapsAtEndOfMemory()
  {
    bus.Write(0xFFFF, 0x34);
    bus.Write(0x0000, 0x12);

    Assert.Equal(0x1234, bus.Read16Bits(0xFFFF));
  }

  [Fact]
  public void WriteAtEdges_PreservesValues()
  {
    bus.Write(0x0000, 0xAA);
    bus.Write(0xFFFF, 0xBB);

    Assert.Equal(0xAA, bus.Read(0x0000));
    Assert.Equal(0xBB, bus.Read(0xFFFF));
  }
}