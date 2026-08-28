namespace Mos6502.Tests.Domain.Entities;

public class LdyTests
{
  [Theory]
  [InlineData(0x05, false, false)]
  [InlineData(0x00, true, false)]
  [InlineData(0x80, false, true)]
  public void Ldy_Immediate_SetsYAndFlags(
    u8 operand,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LdyImmediate(operand);

    cpu.Y.Should().Be(operand);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Ldy_ZeroPage_ReadsMemoryAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x55);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LdyZeroPage(0x42);

    cpu.Y.Should().Be(0x55);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Ldy_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x33);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.LdyZeroPageX(0x80);

    cpu.Y.Should().Be(0x33);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldy_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x77);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x03);

    var cycles = cpu.LdyZeroPageX(0xFF);

    cpu.Y.Should().Be(0x77);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldy_Absolute_ReadsMemoryAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x99);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LdyAbsolute(0x1234);

    cpu.Y.Should().Be(0x99);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldy_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x11);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.LdyAbsoluteX(0x2000);

    cpu.Y.Should().Be(0x11);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldy_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x22);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.LdyAbsoluteX(0x20FF);

    cpu.Y.Should().Be(0x22);
    cycles.Should().Be(5);
  }
}
