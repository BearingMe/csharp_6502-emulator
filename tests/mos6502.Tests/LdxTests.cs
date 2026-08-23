namespace mos6502.Tests;

public class LdxTests
{
  [Theory]
  [InlineData(0x00, true, false)]
  [InlineData(0x42, false, false)]
  [InlineData(0x80, false, true)]
  [InlineData(0xFF, false, true)]
  public void Ldx_LoadsValueAndUpdatesFlags(
    u8 value,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    cpu.LDX_immediate(value);

    cpu.X.Should().Be(value);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
  }

  [Fact]
  public void Ldx_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    cpu.LDX_immediate(0x42);

    cpu.Status.Should().Be(Status.Interrupt);
  }

  [Fact]
  public void Ldx_Immediate_ReadsOperandAndReturnsTwoCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    var cycles = cpu.LDX_immediate(0x37);

    cpu.X.Should().Be(0x37);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Ldx_ZeroPage_ReadsFromAddressAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x55);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDX_zero_page(0x42);

    cpu.X.Should().Be(0x55);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Ldx_ZeroPageY_AppliesYOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0080, 0x66);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDX_zero_page_y(0x80);

    cpu.X.Should().Be(0x66);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldx_ZeroPageY_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x77);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDX_zero_page_y(0xFF);

    cpu.X.Should().Be(0x77);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldx_Absolute_ReadsFromAddressAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x88);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDX_absolute(0x1234);

    cpu.X.Should().Be(0x88);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldx_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0x99);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDX_absolute_y(0x2000);

    cpu.X.Should().Be(0x99);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldx_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0xAA);
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x05);

    var cycles = cpu.LDX_absolute_y(0x20FF);

    cpu.X.Should().Be(0xAA);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Ldx_ZeroPageY_AppliesNonZeroYOffset()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x44);
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x05);

    var cycles = cpu.LDX_zero_page_y(0x80);

    cpu.X.Should().Be(0x44);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldx_ZeroPageY_WrapsWithinZeroPageWithNonZeroY()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x33);
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x03);

    var cycles = cpu.LDX_zero_page_y(0xFF);

    cpu.X.Should().Be(0x33);
    cycles.Should().Be(4);
  }
}
