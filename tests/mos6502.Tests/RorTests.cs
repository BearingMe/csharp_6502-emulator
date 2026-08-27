namespace mos6502.Tests;

public class RorTests
{
  [Theory]
  [InlineData(0x02, false, 0x01, false, false, false)]
  [InlineData(0x02, true, 0x81, false, false, true)]
  [InlineData(0x01, false, 0x00, true, true, false)]
  [InlineData(0x01, true, 0x80, true, false, true)]
  [InlineData(0x80, false, 0x40, false, false, false)]
  [InlineData(0x80, true, 0xC0, false, false, true)]
  [InlineData(0xFF, false, 0x7F, true, false, false)]
  [InlineData(0xFF, true, 0xFF, true, false, true)]
  [InlineData(0x00, false, 0x00, false, true, false)]
  [InlineData(0x00, true, 0x80, false, false, true)]
  public void Ror_Accumulator_RotatesRightThroughCarryAndUpdatesFlags(
    u8 initial,
    bool initialCarry,
    u8 expected,
    bool expectedCarry,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    if (initialCarry)
    {
      cpu.LDA_immediate(0xFF);
      cpu.ADC_immediate(0x01); // sets Carry flag
    }
    cpu.LDA_immediate(initial);

    var cycles = cpu.ROR_accumulator();

    cpu.A.Should().Be(expected);
    cpu.Status.HasFlag(Status.Carry).Should().Be(expectedCarry);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Ror_Accumulator_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x02);

    cpu.ROR_accumulator();

    cpu.Status.Should().Be(Status.Interrupt);
  }

  [Fact]
  public void Ror_ZeroPage_ReadsModifiesAndWritesMemoryAndReturnsFiveCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x81);
    var cpu = new Emulator(bus);

    var cycles = cpu.ROR_zero_page(0x42);

    bus.ReadByte(0x0042).Should().Be(0x40);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Ror_ZeroPageX_AppliesXOffsetAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0084, 0x80);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.ROR_zero_page_x(0x80);

    bus.ReadByte(0x0084).Should().Be(0x40);
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cpu.Status.HasFlag(Status.Carry).Should().BeFalse();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Ror_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0004, 0x02);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.ROR_zero_page_x(0xFF);

    bus.ReadByte(0x0004).Should().Be(0x01);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Ror_Absolute_ReadsModifiesAndWritesMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0xC1);
    var cpu = new Emulator(bus);

    var cycles = cpu.ROR_absolute(0x2000);

    bus.ReadByte(0x2000).Should().Be(0x60);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Ror_AbsoluteX_WithoutPageCrossing_ReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2005, 0x04);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.ROR_absolute_x(0x2000);

    bus.ReadByte(0x2005).Should().Be(0x02);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Ror_AbsoluteX_WithPageCrossing_ReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2102, 0x01);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.ROR_absolute_x(0x20FD);

    bus.ReadByte(0x2102).Should().Be(0x00);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(7);
  }
}
