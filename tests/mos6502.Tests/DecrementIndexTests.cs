namespace mos6502.Tests;

public class DecrementIndexTests
{
  [Theory]
  [InlineData(0x02, 0x01, false, false)]
  [InlineData(0x01, 0x00, true, false)]
  [InlineData(0x00, 0xFF, false, true)]
  [InlineData(0x80, 0x7F, false, false)]
  public void Dex_DecrementsX_AndUpdatesFlags(
    u8 initial,
    u8 expected,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(initial);

    var cycles = cpu.DEX();

    cpu.X.Should().Be(expected);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Dex_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x42);

    cpu.DEX();

    cpu.Status.Should().Be(Status.Interrupt);
  }

  [Theory]
  [InlineData(0x02, 0x01, false, false)]
  [InlineData(0x01, 0x00, true, false)]
  [InlineData(0x00, 0xFF, false, true)]
  [InlineData(0x80, 0x7F, false, false)]
  public void Dey_DecrementsY_AndUpdatesFlags(
    u8 initial,
    u8 expected,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(initial);

    var cycles = cpu.DEY();

    cpu.Y.Should().Be(expected);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Dey_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x42);

    cpu.DEY();

    cpu.Status.Should().Be(Status.Interrupt);
  }
}
