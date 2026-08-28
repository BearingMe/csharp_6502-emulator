namespace Mos6502.Tests.Domain.Entities;

public class DecTests
{
  [Fact]
  public void Dec_ZeroPage_DecrementsMemoryAndSetsFlags()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x05);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.DecZeroPage(0x42);

    bus.ReadByte(0x0042).Should().Be(0x04);
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Dec_ZeroPage_SetsZeroFlag_WhenResultIsZero()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x01);
    var cpu = new Mos6502.Application.Emulator(bus);

    cpu.DecZeroPage(0x42);

    bus.ReadByte(0x0042).Should().Be(0x00);
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
  }

  [Fact]
  public void Dec_ZeroPage_SetsNegativeFlag_WhenResultIsNegative()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x00);
    var cpu = new Mos6502.Application.Emulator(bus);

    cpu.DecZeroPage(0x42);

    bus.ReadByte(0x0042).Should().Be(0xFF);
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
  }

  [Fact]
  public void Dec_ZeroPageX_AppliesXOffsetAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0045, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.DecZeroPageX(0x40);

    bus.ReadByte(0x0045).Should().Be(0x0F);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Dec_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x08);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x03);

    var cycles = cpu.DecZeroPageX(0xFF);

    bus.ReadByte(0x0002).Should().Be(0x07);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Dec_Absolute_ModifiesMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0x20);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.DecAbsolute(0x2000);

    bus.ReadByte(0x2000).Should().Be(0x1F);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Dec_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x09);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.DecAbsoluteX(0x2000);

    bus.ReadByte(0x2004).Should().Be(0x08);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Dec_AbsoluteX_HasFixedSevenCyclesEvenWhenPageCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x09);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.DecAbsoluteX(0x20FF);

    bus.ReadByte(0x2104).Should().Be(0x08);
    cycles.Should().Be(7);
  }
}
