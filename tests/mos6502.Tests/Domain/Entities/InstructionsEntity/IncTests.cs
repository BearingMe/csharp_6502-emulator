namespace Mos6502.Tests.Domain.Entities;

public class IncTests
{
  [Fact]
  public void Inc_ZeroPage_IncrementsMemoryAndSetsFlags()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x05);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.IncZeroPage(0x42);

    bus.ReadByte(0x0042).Should().Be(0x06);
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Inc_ZeroPage_SetsZeroFlag_WhenResultWrapsToZero()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0xFF);
    var cpu = new Mos6502.Application.Emulator(bus);

    cpu.IncZeroPage(0x42);

    bus.ReadByte(0x0042).Should().Be(0x00);
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
  }

  [Fact]
  public void Inc_ZeroPage_SetsNegativeFlag_WhenResultIsNegative()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x7F);
    var cpu = new Mos6502.Application.Emulator(bus);

    cpu.IncZeroPage(0x42);

    bus.ReadByte(0x0042).Should().Be(0x80);
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
  }

  [Fact]
  public void Inc_ZeroPageX_AppliesXOffsetAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0045, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.IncZeroPageX(0x40);

    bus.ReadByte(0x0045).Should().Be(0x11);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Inc_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x08);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x03);

    var cycles = cpu.IncZeroPageX(0xFF);

    bus.ReadByte(0x0002).Should().Be(0x09);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Inc_Absolute_ModifiesMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0x20);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.IncAbsolute(0x2000);

    bus.ReadByte(0x2000).Should().Be(0x21);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Inc_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x09);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.IncAbsoluteX(0x2000);

    bus.ReadByte(0x2004).Should().Be(0x0A);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Inc_AbsoluteX_HasFixedSevenCyclesEvenWhenPageCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x09);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.IncAbsoluteX(0x20FF);

    bus.ReadByte(0x2104).Should().Be(0x0A);
    cycles.Should().Be(7);
  }
}
