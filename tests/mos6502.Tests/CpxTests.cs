namespace mos6502.Tests;

public class CpxTests
{
  [Theory]
  [InlineData(0x40, 0x20, true, false, false)]
  [InlineData(0x40, 0x40, true, true, false)]
  [InlineData(0x40, 0x60, false, false, true)]
  public void Cpx_ComparesXWithOperand_AndUpdatesFlags(
    u8 initialX,
    u8 operand,
    bool carry,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(initialX);

    var cycles = cpu.CPX_immediate(operand);

    cpu.X.Should().Be(initialX);
    cpu.Status.HasFlag(Status.Carry).Should().Be(carry);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Cpx_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x50);
    cpu.ADC_immediate(0x50); // sets Overflow
    cpu.LDX_immediate(0x40);

    cpu.CPX_immediate(0x40);

    cpu.Status.Should().Be(Status.Interrupt | Status.Carry | Status.Zero | Status.Overflow);
  }

  [Fact]
  public void Cpx_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x40);

    var cycles = cpu.CPX_zero_page(0x42);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(3);
  }

  [Fact]
  public void Cpx_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x40);

    var cycles = cpu.CPX_absolute(0x1234);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(4);
  }
}
