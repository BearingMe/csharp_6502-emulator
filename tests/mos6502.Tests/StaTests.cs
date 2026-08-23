namespace mos6502.Tests;

public class StaTests
{
  [Fact]
  public void Sta_PreservesStatusFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x00);

    cpu.STA_zero_page(0x10);

    cpu.Status.Should().Be(Status.Interrupt | Status.Zero);
  }

  [Fact]
  public void Sta_ZeroPage_WritesAccumulatorToAddressAndReturnsThreeCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);

    var cycles = cpu.STA_zero_page(0x80);

    bus.ReadByte(0x0080).Should().Be(0x42);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Sta_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.STA_zero_page_x(0x80);

    bus.ReadByte(0x0085).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sta_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDX_immediate(0x03);

    var cycles = cpu.STA_zero_page_x(0xFF);

    bus.ReadByte(0x0002).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sta_Absolute_WritesAccumulatorToAddressAndReturnsFourCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);

    var cycles = cpu.STA_absolute(0x1234);

    bus.ReadByte(0x1234).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sta_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.STA_absolute_x(0x2000);

    bus.ReadByte(0x2004).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sta_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.STA_absolute_x(0x20FF);

    bus.ReadByte(0x2104).Should().Be(0x42);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sta_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDY_immediate(0x08);

    var cycles = cpu.STA_absolute_y(0x3000);

    bus.ReadByte(0x3008).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sta_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDY_immediate(0x02);

    var cycles = cpu.STA_absolute_y(0x30FF);

    bus.ReadByte(0x3101).Should().Be(0x42);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sta_IndexedIndirect_AppliesXOffsetAndWritesToIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.STA_indexed_indirect(0x20);

    bus.ReadByte(0x1234).Should().Be(0x42);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Sta_IndexedIndirect_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0000, 0x00);
    bus.WriteByte(0x0001, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDX_immediate(0x01);

    var cycles = cpu.STA_indexed_indirect(0xFF);

    bus.ReadByte(0x4000).Should().Be(0x42);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Sta_IndexedIndirect_WrapsHighByteAtEndOfZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x34);
    bus.WriteByte(0x0000, 0x12);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);

    var cycles = cpu.STA_indexed_indirect(0xFF);

    bus.ReadByte(0x1234).Should().Be(0x42);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Sta_IndirectIndexed_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDY_immediate(0x04);

    var cycles = cpu.STA_indirect_indexed(0x40);

    bus.ReadByte(0x5004).Should().Be(0x42);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sta_IndirectIndexed_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x34);
    bus.WriteByte(0x0000, 0x12);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);

    var cycles = cpu.STA_indirect_indexed(0xFF);

    bus.ReadByte(0x1234).Should().Be(0x42);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sta_IndirectIndexed_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0xFF);
    bus.WriteByte(0x0041, 0x50);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDY_immediate(0x02);

    var cycles = cpu.STA_indirect_indexed(0x40);

    bus.ReadByte(0x5101).Should().Be(0x42);
    cycles.Should().Be(6);
  }
}
