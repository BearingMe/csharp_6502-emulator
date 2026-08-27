namespace mos6502.Tests;

public class StackTests
{
  [Fact]
  public void Pha_PushesAccumulatorToStack_DecrementsStackPointerAndReturnsThreeCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);

    var cycles = cpu.PHA();

    bus.ReadByte(0x01FD).Should().Be(0x42);
    cpu.StackPointer.Should().Be(0xFC);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Pha_PreservesFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x00); // sets Zero flag

    cpu.PHA();

    cpu.Status.Should().Be(Status.Interrupt | Status.Zero);
  }

  [Fact]
  public void Pha_AtStackPointerZero_WrapsToStackPointerFFAndWritesTo0100()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x00);
    cpu.TXS();
    cpu.LDA_immediate(0x99);

    var cycles = cpu.PHA();

    bus.ReadByte(0x0100).Should().Be(0x99);
    cpu.StackPointer.Should().Be(0xFF);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Php_PushesStatusWithBreakAndUnusedBitsSet_DecrementsStackPointerAndReturnsThreeCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0xFF);
    cpu.ADC_immediate(0x01); // sets Carry and Zero flags

    var cycles = cpu.PHP();

    // Bits 4 (Break) and 5 (Unused) must be set on stack, along with default Interrupt flag (0x30 | Carry | Zero | Interrupt = 0x37)
    bus.ReadByte(0x01FD).Should().Be((u8)(Status.Carry | Status.Zero | Status.Interrupt | Status.Break | Status.Unused));
    cpu.StackPointer.Should().Be(0xFC);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.Status.HasFlag(Status.Interrupt).Should().BeTrue();
    cycles.Should().Be(3);
  }

  [Fact]
  public void Php_PreservesFlagsInStatusRegister()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x80); // sets Negative flag

    cpu.PHP();

    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
  }

  [Theory]
  [InlineData(0x00, true, false)]
  [InlineData(0x01, false, false)]
  [InlineData(0x7F, false, false)]
  [InlineData(0x80, false, true)]
  [InlineData(0xFF, false, true)]
  public void Pla_PullsAccumulatorFromStack_IncrementsStackPointerUpdatesFlagsAndReturnsFourCycles(
    u8 value,
    bool expectedZero,
    bool expectedNegative)
  {
    var bus = new Bus();
    bus.WriteByte(0x01FD, value);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0xFC);
    cpu.TXS();

    var cycles = cpu.PLA();

    cpu.A.Should().Be(value);
    cpu.StackPointer.Should().Be(0xFD);
    cpu.Status.HasFlag(Status.Zero).Should().Be(expectedZero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(expectedNegative);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Pla_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    bus.WriteByte(0x01FD, 0x42);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0xFF);
    cpu.ADC_immediate(0x01); // sets Carry
    cpu.LDX_immediate(0xFC);
    cpu.TXS();

    cpu.PLA();

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
  }

  [Fact]
  public void Pla_AtStackPointerFF_WrapsToStackPointerZeroAndReadsFrom0100()
  {
    var bus = new Bus();
    bus.WriteByte(0x0100, 0x55);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0xFF);
    cpu.TXS();

    var cycles = cpu.PLA();

    cpu.A.Should().Be(0x55);
    cpu.StackPointer.Should().Be(0x00);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Plp_PullsStatusFromStack_IgnoresBreakAndUnusedBitsAndReturnsFourCycles()
  {
    var bus = new Bus();
    // Pushed byte with bits 7,6,3,2,1,0 set plus bit 4 (Break) and bit 5 (Unused)
    bus.WriteByte(0x01FD, 0xFF);
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0xFC);
    cpu.TXS();

    var cycles = cpu.PLP();

    // Break (bit 4) and Unused (bit 5) should remain untouched from previous CPU status, other bits restored from stack
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.Status.HasFlag(Status.Interrupt).Should().BeTrue();
    cpu.Status.HasFlag(Status.Decimal).Should().BeTrue();
    cpu.Status.HasFlag(Status.Overflow).Should().BeTrue();
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
    cpu.Status.HasFlag(Status.Break).Should().BeFalse();
    cpu.StackPointer.Should().Be(0xFD);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Plp_DoesNotModifyAccumulatorOrOtherRegisters()
  {
    var bus = new Bus();
    bus.WriteByte(0x01FD, (u8)Status.Negative);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDX_immediate(0xFC);
    cpu.TXS();
    cpu.LDY_immediate(0x20);

    cpu.PLP();

    cpu.A.Should().Be(0x42);
    cpu.Y.Should().Be(0x20);
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
  }
}
