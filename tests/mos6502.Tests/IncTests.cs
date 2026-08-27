namespace mos6502.Tests;

public class IncTests
{
  [Theory]
  [InlineData(0x00, 0x01, false, false)]
  [InlineData(0x7F, 0x80, false, true)]
  [InlineData(0xFE, 0xFF, false, true)]
  [InlineData(0xFF, 0x00, true, false)]
  public void Inc_ZeroPage_IncrementsMemoryAndUpdatesFlags(
    u8 initial,
    u8 expected,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, initial);
    var cpu = new Emulator(bus);

    var cycles = cpu.INC_zero_page(0x42);

    bus.ReadByte(0x0042).Should().Be(expected);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Inc_ZeroPage_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x01);
    var cpu = new Emulator(bus);

    cpu.INC_zero_page(0x42);

    cpu.Status.Should().Be(Status.Interrupt);
  }

  [Fact]
  public void Inc_ZeroPageX_AppliesXOffsetAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0084, 0x7F);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.INC_zero_page_x(0x80);

    bus.ReadByte(0x0084).Should().Be(0x80);
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Inc_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0004, 0x05);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.INC_zero_page_x(0xFF);

    bus.ReadByte(0x0004).Should().Be(0x06);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Inc_Absolute_IncrementsMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0x41);
    var cpu = new Emulator(bus);

    var cycles = cpu.INC_absolute(0x2000);

    bus.ReadByte(0x2000).Should().Be(0x42);
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Inc_AbsoluteX_WithoutPageCrossing_ReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2005, 0x02);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.INC_absolute_x(0x2000);

    bus.ReadByte(0x2005).Should().Be(0x03);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Inc_AbsoluteX_WithPageCrossing_ReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2102, 0xFF);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.INC_absolute_x(0x20FD);

    bus.ReadByte(0x2102).Should().Be(0x00);
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(7);
  }
}
