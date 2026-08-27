namespace mos6502.Tests;

public class DecTests
{
  [Theory]
  [InlineData(0x01, 0x00, true, false)]
  [InlineData(0x02, 0x01, false, false)]
  [InlineData(0x00, 0xFF, false, true)]
  [InlineData(0x80, 0x7F, false, false)]
  [InlineData(0xFF, 0xFE, false, true)]
  public void Dec_ZeroPage_DecrementsMemoryAndUpdatesFlags(
    u8 initial,
    u8 expected,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, initial);
    var cpu = new Emulator(bus);

    var cycles = cpu.DEC_zero_page(0x42);

    bus.ReadByte(0x0042).Should().Be(expected);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Dec_ZeroPage_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x05);
    var cpu = new Emulator(bus);

    cpu.DEC_zero_page(0x42);

    cpu.Status.Should().Be(Status.Interrupt);
  }

  [Fact]
  public void Dec_ZeroPageX_AppliesXOffsetAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0084, 0x01);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.DEC_zero_page_x(0x80);

    bus.ReadByte(0x0084).Should().Be(0x00);
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Dec_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0004, 0x05);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.DEC_zero_page_x(0xFF);

    bus.ReadByte(0x0004).Should().Be(0x04);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Dec_Absolute_DecrementsMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0x42);
    var cpu = new Emulator(bus);

    var cycles = cpu.DEC_absolute(0x2000);

    bus.ReadByte(0x2000).Should().Be(0x41);
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Dec_AbsoluteX_WithoutPageCrossing_ReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2005, 0x02);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.DEC_absolute_x(0x2000);

    bus.ReadByte(0x2005).Should().Be(0x01);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Dec_AbsoluteX_WithPageCrossing_ReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2102, 0x00);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.DEC_absolute_x(0x20FD);

    bus.ReadByte(0x2102).Should().Be(0xFF);
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
    cycles.Should().Be(7);
  }
}
