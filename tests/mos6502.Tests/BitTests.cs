namespace mos6502.Tests;

public class BitTests
{
  [Theory]
  [InlineData(0b0000_1111, 0b0001_0000, true, false, false)]
  [InlineData(0b0000_1111, 0b0101_0000, true, true, false)]
  [InlineData(0b0000_1111, 0b1001_0000, true, false, true)]
  [InlineData(0b0000_1111, 0b1101_0000, true, true, true)]
  [InlineData(0b0000_1111, 0b0000_1111, false, false, false)]
  [InlineData(0b0100_0001, 0b0100_0001, false, true, false)]
  [InlineData(0b1000_0001, 0b1000_0001, false, false, true)]
  [InlineData(0b1100_0001, 0b1100_0001, false, true, true)]
  public void Bit_ZeroPage_UpdatesFlagsAccordingToSpecification(
    u8 initialA,
    u8 memoryValue,
    bool expectedZero,
    bool expectedOverflow,
    bool expectedNegative)
  {
    var bus = new Bus();
    bus.WriteByte(0x0010, memoryValue);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(initialA);

    var cycles = cpu.Bit_zero_page(0x10);

    cpu.Status.HasFlag(Status.Zero).Should().Be(expectedZero);
    cpu.Status.HasFlag(Status.Overflow).Should().Be(expectedOverflow);
    cpu.Status.HasFlag(Status.Negative).Should().Be(expectedNegative);
    cpu.A.Should().Be(initialA);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Bit_DoesNotModifyAccumulatorOrRegisters()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0b1100_0000);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x3C);
    cpu.LDX_immediate(0x10);
    cpu.LDY_immediate(0x20);

    cpu.Bit_zero_page(0x42);

    cpu.A.Should().Be(0x3C);
    cpu.X.Should().Be(0x10);
    cpu.Y.Should().Be(0x20);
  }

  [Fact]
  public void Bit_DoesNotModifyMemory()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0xAA);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x55);

    cpu.Bit_zero_page(0x42);

    bus.ReadByte(0x0042).Should().Be(0xAA);
  }

  [Fact]
  public void Bit_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    bus.WriteByte(0x0010, 0x00);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0xFF);
    cpu.ADC_immediate(0x01); // sets Carry and Zero
    cpu.LDA_immediate(0x00); // clears Zero, preserves Carry

    cpu.Bit_zero_page(0x10);

    cpu.Status.Should().HaveFlag(Status.Carry);
    cpu.Status.Should().HaveFlag(Status.Interrupt);
  }

  [Fact]
  public void Bit_Absolute_ReadsOperandAndSetsFlags()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0b1100_0000);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0b0100_0000);

    var cycles = cpu.Bit_absolute(0x1234);

    cpu.A.Should().Be(0b0100_0000);
    cpu.Status.Should().NotHaveFlag(Status.Zero);
    cpu.Status.Should().HaveFlag(Status.Overflow);
    cpu.Status.Should().HaveFlag(Status.Negative);
    cycles.Should().Be(4);
  }
}
