namespace Mos6502.Tests.Domain.Entities;

public class LsrTests
{
  [Fact]
  public void Lsr_Accumulator_ShiftsRightAndSetsCarry()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x81); // 10000001 -> 01000000, carry=1

    var cycles = cpu.LsrAccumulator();

    cpu.A.Should().Be(0x40);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Lsr_Accumulator_SetsZeroAndClearsNegativeFlags()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x01); // 00000001 -> 00000000, carry=1, zero=1

    cpu.LsrAccumulator();

    cpu.A.Should().Be(0x00);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
  }

  [Fact]
  public void Lsr_ZeroPage_ModifiesMemoryAndReturnsFiveCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x02);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LsrZeroPage(0x42);

    bus.ReadByte(0x0042).Should().Be(0x01);
    cpu.Status.HasFlag(Status.Carry).Should().BeFalse();
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Lsr_ZeroPageX_AppliesXOffsetAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0045, 0x04);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.LsrZeroPageX(0x40);

    bus.ReadByte(0x0045).Should().Be(0x02);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Lsr_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x08);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x03);

    var cycles = cpu.LsrZeroPageX(0xFF);

    bus.ReadByte(0x0002).Should().Be(0x04);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Lsr_Absolute_ModifiesMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LsrAbsolute(0x2000);

    bus.ReadByte(0x2000).Should().Be(0x08);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Lsr_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x08);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.LsrAbsoluteX(0x2000);

    bus.ReadByte(0x2004).Should().Be(0x04);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Lsr_AbsoluteX_HasFixedSevenCyclesEvenWhenPageCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x08);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.LsrAbsoluteX(0x20FF);

    bus.ReadByte(0x2104).Should().Be(0x04);
    cycles.Should().Be(7);
  }
}
