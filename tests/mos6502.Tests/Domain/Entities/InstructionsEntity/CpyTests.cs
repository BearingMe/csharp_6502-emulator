namespace Mos6502.Tests.Domain.Entities;

public class CpyTests
{
  [Theory]
  [InlineData(0x20, 0x10, true, false, false)] // Y > M -> C=1, Z=0, N=0
  [InlineData(0x10, 0x20, false, false, true)] // Y < M -> C=0, Z=0, N=1
  [InlineData(0x10, 0x10, true, true, false)]  // Y == M -> C=1, Z=1, N=0
  public void Cpy_ComparesYAndMemory_AndSetsFlags(
    u8 y,
    u8 operand,
    bool carry,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(y);

    var cycles = cpu.CpyImmediate(operand);

    cpu.Status.HasFlag(Status.Carry).Should().Be(carry);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Cpy_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x20);

    var cycles = cpu.CpyZeroPage(0x42);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(3);
  }

  [Fact]
  public void Cpy_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x20);

    var cycles = cpu.CpyAbsolute(0x1234);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(4);
  }
}
