namespace mos6502.Tests;

public class LsrTests
{
  [Theory]
  [InlineData(0x02, 0x01, false, false)]
  [InlineData(0x01, 0x00, true, true)]
  [InlineData(0x80, 0x40, false, false)]
  [InlineData(0x81, 0x40, true, false)]
  [InlineData(0xFF, 0x7F, true, false)]
  [InlineData(0x00, 0x00, false, true)]
  public void Lsr_Accumulator_ShiftsRightAndUpdatesFlags(
    u8 initial,
    u8 expected,
    bool carry,
    bool zero)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(initial);

    var cycles = cpu.LSR_accumulator();

    cpu.A.Should().Be(expected);
    cpu.Status.HasFlag(Status.Carry).Should().Be(carry);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Lsr_Accumulator_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x02);

    cpu.LSR_accumulator();

    cpu.Status.Should().Be(Status.Interrupt);
  }

  [Fact]
  public void Lsr_ZeroPage_ReadsModifiesAndWritesMemoryAndReturnsFiveCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x81);
    var cpu = new Emulator(bus);

    var cycles = cpu.LSR_zero_page(0x42);

    bus.ReadByte(0x0042).Should().Be(0x40);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Lsr_ZeroPageX_AppliesXOffsetAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0084, 0x01);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.LSR_zero_page_x(0x80);

    bus.ReadByte(0x0084).Should().Be(0x00);
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Lsr_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0004, 0x04);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.LSR_zero_page_x(0xFF);

    bus.ReadByte(0x0004).Should().Be(0x02);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Lsr_Absolute_ReadsModifiesAndWritesMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0xFE);
    var cpu = new Emulator(bus);

    var cycles = cpu.LSR_absolute(0x2000);

    bus.ReadByte(0x2000).Should().Be(0x7F);
    cpu.Status.HasFlag(Status.Carry).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Lsr_AbsoluteX_WithoutPageCrossing_ReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2005, 0x03);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.LSR_absolute_x(0x2000);

    bus.ReadByte(0x2005).Should().Be(0x01);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(7);
  }

  [Fact]
  public void Lsr_AbsoluteX_WithPageCrossing_ReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x2102, 0x01);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.LSR_absolute_x(0x20FD);

    bus.ReadByte(0x2102).Should().Be(0x00);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(7);
  }
}
