namespace Mos6502.Tests.Domain.Entities;

public class CpxTests
{
  [Theory]
  [InlineData(0x20, 0x10, true, false, false)] // X > M -> C=1, Z=0, N=0
  [InlineData(0x10, 0x20, false, false, true)] // X < M -> C=0, Z=0, N=1
  [InlineData(0x10, 0x10, true, true, false)]  // X == M -> C=1, Z=1, N=0
  public void Cpx_ComparesXAndMemory_AndSetsFlags(
    u8 x,
    u8 operand,
    bool carry,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(x);

    var cycles = cpu.CpxImmediate(operand);

    cpu.Status.HasFlag(Status.Carry).Should().Be(carry);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Cpx_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x20);

    var cycles = cpu.CpxZeroPage(0x42);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(3);
  }

  [Fact]
  public void Cpx_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x20);

    var cycles = cpu.CpxAbsolute(0x1234);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(4);
  }
}
