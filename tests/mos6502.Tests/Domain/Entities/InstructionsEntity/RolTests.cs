namespace Mos6502.Tests.Domain.Entities;

public class RolTests
{
  [Fact]
  public void Rol_Accumulator_RotatesLeftWithCarryClear()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = 0;
    cpu.LdaImmediate(0x81); // 10000001 -> 00000010, carry=1

    var cycles = cpu.RolAccumulator();

    cpu.A.Should().Be(0x02);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Rol_Accumulator_RotatesLeftWithCarrySet()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = Status.Carry;
    cpu.LdaImmediate(0x00); // 00000000 + carry -> 00000001, carry=0

    cpu.RolAccumulator();

    cpu.A.Should().Be(0x01);
    cpu.Status.HasFlag(Status.Carry).Should().BeFalse();
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
  }

  [Fact]
  public void Rol_ZeroPage_ModifiesMemoryAndReturnsFiveCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x40);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = Status.Carry;

    var cycles = cpu.RolZeroPage(0x42); // 01000000 + C -> 10000001, C=0, N=1

    bus.ReadByte(0x0042).Should().Be(0x81);
    cpu.Status.HasFlag(Status.Carry).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Rol_ZeroPageX_AppliesXOffsetAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0045, 0x02);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.RolZeroPageX(0x40);

    bus.ReadByte(0x0045).Should().Be(0x04);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Rol_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x01);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x03);

    var cycles = cpu.RolZeroPageX(0xFF);

    bus.ReadByte(0x0002).Should().Be(0x02);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Rol_Absolute_ModifiesMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0x08);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.RolAbsolute(0x2000);

    bus.ReadByte(0x2000).Should().Be(0x10);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Rol_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x04);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.RolAbsoluteX(0x2000);

    bus.ReadByte(0x2004).Should().Be(0x08);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Rol_AbsoluteX_HasFixedSevenCyclesEvenWhenPageCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x04);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.RolAbsoluteX(0x20FF);

    bus.ReadByte(0x2104).Should().Be(0x08);
    cycles.Should().Be(7);
  }
}
