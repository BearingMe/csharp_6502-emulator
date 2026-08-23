namespace mos6502.Tests;

public class IncrementIndexTests
{
  [Theory]
  [InlineData(0x00, 0x01, false, false)]
  [InlineData(0x7F, 0x80, false, true)]
  [InlineData(0xFE, 0xFF, false, true)]
  [InlineData(0xFF, 0x00, true, false)]
  public void Inx_IncrementsX_AndUpdatesFlags(
    u8 initial,
    u8 expected,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(initial);

    var cycles = cpu.INX();

    cpu.X.Should().Be(expected);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Inx_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x42);

    cpu.INX();

    cpu.Status.Should().Be(Status.Interrupt);
  }

  [Theory]
  [InlineData(0x00, 0x01, false, false)]
  [InlineData(0x7F, 0x80, false, true)]
  [InlineData(0xFE, 0xFF, false, true)]
  [InlineData(0xFF, 0x00, true, false)]
  public void Iny_IncrementsY_AndUpdatesFlags(
    u8 initial,
    u8 expected,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(initial);

    var cycles = cpu.INY();

    cpu.Y.Should().Be(expected);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Iny_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x42);

    cpu.INY();

    cpu.Status.Should().Be(Status.Interrupt);
  }
}
