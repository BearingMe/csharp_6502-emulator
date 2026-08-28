namespace Mos6502.Tests.Domain.Entities;

public class DecrementIndexTests
{
  [Theory]
  [InlineData(0x05, 0x04, false, false)]
  [InlineData(0x01, 0x00, true, false)]
  [InlineData(0x00, 0xFF, false, true)]
  public void Dex_DecrementsX_AndUpdatesFlags(
    u8 initialX,
    u8 expectedX,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(initialX);

    var cycles = cpu.Dex();

    cpu.X.Should().Be(expectedX);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Theory]
  [InlineData(0x05, 0x04, false, false)]
  [InlineData(0x01, 0x00, true, false)]
  [InlineData(0x00, 0xFF, false, true)]
  public void Dey_DecrementsY_AndUpdatesFlags(
    u8 initialY,
    u8 expectedY,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(initialY);

    var cycles = cpu.Dey();

    cpu.Y.Should().Be(expectedY);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }
}
