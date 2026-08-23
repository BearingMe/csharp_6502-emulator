namespace mos6502.Tests;

public class EorTests
{
  [Theory]
  [InlineData(0b1111_0000, 0b1010_1010, 0b0101_1010, false, false)]
  [InlineData(0b0000_1111, 0b1000_0000, 0b1000_1111, false, true)]
  [InlineData(0b1010_1010, 0b1010_1010, 0b0000_0000, true, false)]
  public void Eor_PerformsBitwiseExclusiveOr_AndUpdatesFlags(
    u8 initialA,
    u8 operand,
    u8 expectedA,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(initialA);

    var cycles = cpu.EOR_immediate(operand);

    cpu.A.Should().Be(expectedA);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Eor_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0xFF);
    cpu.ADC_immediate(0x01); // sets Carry and Zero
    cpu.LDA_immediate(0x0F);

    cpu.EOR_immediate(0x0F);

    cpu.Status.Should().Be(Status.Interrupt | Status.Carry | Status.Zero);
  }

  [Fact]
  public void Eor_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);

    var cycles = cpu.EOR_zero_page(0x42);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Eor_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.EOR_zero_page_x(0x80);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Eor_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);
    cpu.LDX_immediate(0x03);

    var cycles = cpu.EOR_zero_page_x(0xFF);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Eor_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);

    var cycles = cpu.EOR_absolute(0x1234);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Eor_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.EOR_absolute_x(0x2000);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Eor_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.EOR_absolute_x(0x20FF);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Eor_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x3008, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);
    cpu.LDY_immediate(0x08);

    var cycles = cpu.EOR_absolute_y(0x3000);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Eor_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x3101, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);
    cpu.LDY_immediate(0x02);

    var cycles = cpu.EOR_absolute_y(0x30FF);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Eor_IndexedIndirect_AppliesXOffsetAndReadsIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    bus.WriteByte(0x1234, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.EOR_indexed_indirect(0x20);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Eor_IndexedIndirect_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0000, 0x00);
    bus.WriteByte(0x0001, 0x40);
    bus.WriteByte(0x4000, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);
    cpu.LDX_immediate(0x01);

    var cycles = cpu.EOR_indexed_indirect(0xFF);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Eor_IndirectIndexed_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5004, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);
    cpu.LDY_immediate(0x04);

    var cycles = cpu.EOR_indirect_indexed(0x40);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Eor_IndirectIndexed_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x34);
    bus.WriteByte(0x0000, 0x12);
    bus.WriteByte(0x1234, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);

    var cycles = cpu.EOR_indirect_indexed(0xFF);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Eor_IndirectIndexed_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0xFF);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5101, 0x3C);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x0F);
    cpu.LDY_immediate(0x02);

    var cycles = cpu.EOR_indirect_indexed(0x40);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(6);
  }
}
