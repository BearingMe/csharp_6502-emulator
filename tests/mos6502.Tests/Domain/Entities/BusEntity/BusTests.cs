namespace Mos6502.Tests.Domain.Entities;

public class BusTests
{
  [Fact]
  public void ReadByteAndWriteByte_StoreAndRetrieveMemoryValue()
  {
    var bus = new Mos6502.Domain.Entities.Bus();

    bus.WriteByte(0x1234, 0x42);

    bus.ReadByte(0x1234).Should().Be(0x42);
  }

  [Fact]
  public void ReadWord_CombinesLittleEndianBytes()
  {
    var bus = new Mos6502.Domain.Entities.Bus();
    bus.WriteByte(0x0200, 0x34);
    bus.WriteByte(0x0201, 0x12);

    bus.ReadWord(0x0200).Should().Be(0x1234);
  }

  [Fact]
  public void ReadWord_WrapsAtAddressSpaceEnd()
  {
    var bus = new Mos6502.Domain.Entities.Bus();
    bus.WriteByte(0xFFFF, 0xAA);
    bus.WriteByte(0x0000, 0xBB);

    bus.ReadWord(0xFFFF).Should().Be(0xBBAA);
  }
}
