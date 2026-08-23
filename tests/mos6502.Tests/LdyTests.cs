namespace mos6502.Tests;

public class LdyTests
{
  [Theory]
  [InlineData(0x00, true, false)]
  [InlineData(0x42, false, false)]
  [InlineData(0x80, false, true)]
  [InlineData(0xFF, false, true)]
  public void Ldy_LoadsValueAndUpdatesFlags(
    u8 value,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    cpu.LDY_immediate(value);

    cpu.Y.Should().Be(value);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
  }

  [Fact]
  public void Ldy_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    cpu.LDY_immediate(0x42);

    cpu.Status.Should().Be(Status.Interrupt);
  }

  [Fact]
  public void Ldy_Immediate_ReadsOperandAndReturnsTwoCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    var cycles = cpu.LDY_immediate(0x37);

    cpu.Y.Should().Be(0x37);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Ldy_ZeroPage_ReadsFromAddressAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x55);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDY_zero_page(0x42);

    cpu.Y.Should().Be(0x55);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Ldy_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0080, 0x66);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDY_zero_page_x(0x80);

    cpu.Y.Should().Be(0x66);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldy_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x77);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDY_zero_page_x(0xFF);

    cpu.Y.Should().Be(0x77);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldy_Absolute_ReadsFromAddressAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x88);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDY_absolute(0x1234);

    cpu.Y.Should().Be(0x88);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldy_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0x99);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDY_absolute_x(0x2000);

    cpu.Y.Should().Be(0x99);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldy_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0xAA);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.LDY_absolute_x(0x20FF);

    cpu.Y.Should().Be(0xAA);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Ldy_ZeroPageX_AppliesNonZeroXOffset()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x44);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.LDY_zero_page_x(0x80);

    cpu.Y.Should().Be(0x44);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ldy_ZeroPageX_WrapsWithinZeroPageWithNonZeroX()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x33);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x03);

    var cycles = cpu.LDY_zero_page_x(0xFF);

    cpu.Y.Should().Be(0x33);
    cycles.Should().Be(4);
  }
}
