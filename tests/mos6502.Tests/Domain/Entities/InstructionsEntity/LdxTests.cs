namespace Mos6502.Tests.Domain.Entities;

public class LdxTests
{
  [Theory]
  [InlineData(0x05, false, false)]
  [InlineData(0x00, true, false)]
  [InlineData(0x80, false, true)]
  public void Ldx_Immediate_SetsXAndFlags(
    u8 operand,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LdxImmediate(operand);

    cpu.X.Should().Be(operand);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Ldx_ZeroPage_ReadsMemoryAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x55);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LdxZeroPage(0x42);

    cpu.X.Should().Be(0x55);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Ldx_ZeroPageY_AppliesYOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x33);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x05);

    var cycles = cpu.LdxZeroPageY(0x80);

    cpu.X.Should().Be(0x33);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldx_ZeroPageY_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x77);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x03);

    var cycles = cpu.LdxZeroPageY(0xFF);

    cpu.X.Should().Be(0x77);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldx_Absolute_ReadsMemoryAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x99);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LdxAbsolute(0x1234);

    cpu.X.Should().Be(0x99);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldx_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x3008, 0x33);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x08);

    var cycles = cpu.LdxAbsoluteY(0x3000);

    cpu.X.Should().Be(0x33);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldx_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x3101, 0x44);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.LdxAbsoluteY(0x30FF);

    cpu.X.Should().Be(0x44);
    cycles.Should().Be(5);
  }
}
