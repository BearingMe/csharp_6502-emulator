namespace mos6502.Tests;

public class CpyTests
{
  [Theory]
  [InlineData(0x40, 0x20, true, false, false)]
  [InlineData(0x40, 0x40, true, true, false)]
  [InlineData(0x40, 0x60, false, false, true)]
  public void Cpy_ComparesYWithOperand_AndUpdatesFlags(
    u8 initialY,
    u8 operand,
    bool carry,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(initialY);

    var cycles = cpu.CPY_immediate(operand);

    cpu.Y.Should().Be(initialY);
    cpu.Status.HasFlag(Status.Carry).Should().Be(carry);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Cpy_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x50);
    cpu.ADC_immediate(0x50); // sets Overflow
    cpu.LDY_immediate(0x40);

    cpu.CPY_immediate(0x40);

    cpu.Status.Should().Be(Status.Interrupt | Status.Carry | Status.Zero | Status.Overflow);
  }

  [Fact]
  public void Cpy_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x40);

    var cycles = cpu.CPY_zero_page(0x42);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(3);
  }

  [Fact]
  public void Cpy_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x40);

    var cycles = cpu.CPY_absolute(0x1234);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(4);
  }
}
