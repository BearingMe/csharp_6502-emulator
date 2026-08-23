namespace mos6502.Tests;

public class CmpTests
{
  [Theory]
  [InlineData(0x40, 0x20, true, false, false)]
  [InlineData(0x40, 0x40, true, true, false)]
  [InlineData(0x40, 0x60, false, false, true)]
  public void Cmp_ComparesAccumulatorWithOperand_AndUpdatesFlags(
    u8 initialA,
    u8 operand,
    bool carry,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(initialA);

    var cycles = cpu.CMP_immediate(operand);

    cpu.A.Should().Be(initialA);
    cpu.Status.HasFlag(Status.Carry).Should().Be(carry);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Cmp_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x50);
    cpu.ADC_immediate(0x50); // sets Overflow
    cpu.LDA_immediate(0x40);

    cpu.CMP_immediate(0x40);

    cpu.Status.Should().Be(Status.Interrupt | Status.Carry | Status.Zero | Status.Overflow);
  }

  [Fact]
  public void Cmp_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);

    var cycles = cpu.CMP_zero_page(0x42);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(3);
  }

  [Fact]
  public void Cmp_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.CMP_zero_page_x(0x80);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(4);
  }

  [Fact]
  public void Cmp_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);
    cpu.LDX_immediate(0x03);

    var cycles = cpu.CMP_zero_page_x(0xFF);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(4);
  }

  [Fact]
  public void Cmp_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);

    var cycles = cpu.CMP_absolute(0x1234);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(4);
  }

  [Fact]
  public void Cmp_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.CMP_absolute_x(0x2000);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(4);
  }

  [Fact]
  public void Cmp_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.CMP_absolute_x(0x20FF);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Cmp_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x3008, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);
    cpu.LDY_immediate(0x08);

    var cycles = cpu.CMP_absolute_y(0x3000);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(4);
  }

  [Fact]
  public void Cmp_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x3101, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);
    cpu.LDY_immediate(0x02);

    var cycles = cpu.CMP_absolute_y(0x30FF);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Cmp_IndexedIndirect_AppliesXOffsetAndReadsIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    bus.WriteByte(0x1234, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.CMP_indexed_indirect(0x20);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Cmp_IndexedIndirect_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0000, 0x00);
    bus.WriteByte(0x0001, 0x40);
    bus.WriteByte(0x4000, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);
    cpu.LDX_immediate(0x01);

    var cycles = cpu.CMP_indexed_indirect(0xFF);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Cmp_IndirectIndexed_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5004, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);
    cpu.LDY_immediate(0x04);

    var cycles = cpu.CMP_indirect_indexed(0x40);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Cmp_IndirectIndexed_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x34);
    bus.WriteByte(0x0000, 0x12);
    bus.WriteByte(0x1234, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);

    var cycles = cpu.CMP_indirect_indexed(0xFF);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Cmp_IndirectIndexed_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0xFF);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5101, 0x40);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x40);
    cpu.LDY_immediate(0x02);

    var cycles = cpu.CMP_indirect_indexed(0x40);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(6);
  }
}
