namespace Mos6502.Tests.Domain.Entities;

public class IncrementIndexTests
{
  [Theory]
  [InlineData(0x05, 0x06, false, false)]
  [InlineData(0xFF, 0x00, true, false)]
  [InlineData(0x7F, 0x80, false, true)]
  public void Inx_IncrementsX_AndUpdatesFlags(
    u8 initialX,
    u8 expectedX,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(initialX);

    var cycles = cpu.Inx();

    cpu.X.Should().Be(expectedX);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Theory]
  [InlineData(0x05, 0x06, false, false)]
  [InlineData(0xFF, 0x00, true, false)]
  [InlineData(0x7F, 0x80, false, true)]
  public void Iny_IncrementsY_AndUpdatesFlags(
    u8 initialY,
    u8 expectedY,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(initialY);

    var cycles = cpu.Iny();

    cpu.Y.Should().Be(expectedY);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }
}
