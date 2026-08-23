namespace mos6502.Tests;

public class TransferTests
{
  [Theory]
  [InlineData(0x00, true, false)]
  [InlineData(0x42, false, false)]
  [InlineData(0x80, false, true)]
  [InlineData(0xFF, false, true)]
  public void Tax_TransfersAccumulatorToX_AndUpdatesFlags(
    u8 value,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(value);

    var cycles = cpu.TAX();

    cpu.X.Should().Be(value);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Theory]
  [InlineData(0x00, true, false)]
  [InlineData(0x42, false, false)]
  [InlineData(0x80, false, true)]
  [InlineData(0xFF, false, true)]
  public void Tay_TransfersAccumulatorToY_AndUpdatesFlags(
    u8 value,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(value);

    var cycles = cpu.TAY();

    cpu.Y.Should().Be(value);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Theory]
  [InlineData(0x00, true, false)]
  [InlineData(0x42, false, false)]
  [InlineData(0x80, false, true)]
  [InlineData(0xFF, false, true)]
  public void Txa_TransfersXToAccumulator_AndUpdatesFlags(
    u8 value,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(value);

    var cycles = cpu.TXA();

    cpu.A.Should().Be(value);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Theory]
  [InlineData(0x00, true, false)]
  [InlineData(0x42, false, false)]
  [InlineData(0x80, false, true)]
  [InlineData(0xFF, false, true)]
  public void Tya_TransfersYToAccumulator_AndUpdatesFlags(
    u8 value,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(value);

    var cycles = cpu.TYA();

    cpu.A.Should().Be(value);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Tsx_TransfersStackPointerToX_AndUpdatesFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    var cycles = cpu.TSX();

    cpu.X.Should().Be(0xFD);
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Txs_TransfersXToStackPointer_AndDoesNotModifyFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x80);

    var cycles = cpu.TXS();

    cpu.StackPointer.Should().Be(0x80);
    cpu.Status.Should().Be(Status.Interrupt | Status.Negative);
    cycles.Should().Be(2);
  }
}
