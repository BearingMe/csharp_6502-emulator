namespace Mos6502.Tests.Domain.Entities;

public class TransferTests
{
  [Theory]
  [InlineData(0x05, false, false)]
  [InlineData(0x00, true, false)]
  [InlineData(0x80, false, true)]
  public void Tax_TransfersAccumulatorToX_AndUpdatesFlags(
    u8 initialA,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(initialA);

    var cycles = cpu.Tax();

    cpu.X.Should().Be(initialA);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Theory]
  [InlineData(0x05, false, false)]
  [InlineData(0x00, true, false)]
  [InlineData(0x80, false, true)]
  public void Tay_TransfersAccumulatorToY_AndUpdatesFlags(
    u8 initialA,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(initialA);

    var cycles = cpu.Tay();

    cpu.Y.Should().Be(initialA);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Theory]
  [InlineData(0x05, false, false)]
  [InlineData(0x00, true, false)]
  [InlineData(0x80, false, true)]
  public void Txa_TransfersXToAccumulator_AndUpdatesFlags(
    u8 initialX,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(initialX);

    var cycles = cpu.Txa();

    cpu.A.Should().Be(initialX);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Theory]
  [InlineData(0x05, false, false)]
  [InlineData(0x00, true, false)]
  [InlineData(0x80, false, true)]
  public void Tya_TransfersYToAccumulator_AndUpdatesFlags(
    u8 initialY,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(initialY);

    var cycles = cpu.Tya();

    cpu.A.Should().Be(initialY);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Theory]
  [InlineData(0xFD, false, true)]
  [InlineData(0x00, true, false)]
  [InlineData(0x42, false, false)]
  public void Tsx_TransfersStackPointerToX_AndUpdatesFlags(
    u8 initialSp,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.StackPointer = initialSp;

    var cycles = cpu.Tsx();

    cpu.X.Should().Be(initialSp);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Txs_TransfersXToStackPointer_WithoutModifyingFlags()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0xAB);
    cpu.Status = Status.Carry | Status.Zero;
    var flagsBefore = cpu.Status;

    var cycles = cpu.Txs();

    cpu.StackPointer.Should().Be(0xAB);
    cpu.Status.Should().Be(flagsBefore);
    cycles.Should().Be(2);
  }
}
