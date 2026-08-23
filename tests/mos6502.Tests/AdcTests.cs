namespace mos6502.Tests;

public class AdcTests
{
  [Theory]
  [InlineData(0x20, 0x10, 0x30, false, false, false, false)]
  [InlineData(0x00, 0x00, 0x00, false, true, false, false)]
  [InlineData(0x50, 0x50, 0xA0, false, false, true, true)]
  [InlineData(0xD0, 0x90, 0x60, true, false, false, true)]
  [InlineData(0x50, 0x90, 0xE0, false, false, true, false)]
  [InlineData(0xFF, 0x01, 0x00, true, true, false, false)]
  public void Adc_AddsValues_AndUpdatesFlags_WithoutInitialCarry(
    u8 initialA,
    u8 operand,
    u8 expectedA,
    bool carry,
    bool zero,
    bool negative,
    bool overflow)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(initialA);

    var cycles = cpu.ADC_immediate(operand);

    cpu.A.Should().Be(expectedA);
    cpu.Status.HasFlag(Status.Carry).Should().Be(carry);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cpu.Status.HasFlag(Status.Overflow).Should().Be(overflow);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Adc_IncludesCarryFlag_WhenCarryIsSet()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0xFF);
    cpu.ADC_immediate(0x01); // sets Carry flag

    cpu.LDA_immediate(0x10);
    var cycles = cpu.ADC_immediate(0x20); // 0x10 + 0x20 + 1 = 0x31

    cpu.A.Should().Be(0x31);
    cpu.Status.HasFlag(Status.Carry).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Adc_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);

    var cycles = cpu.ADC_zero_page(0x42);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Adc_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.ADC_zero_page_x(0x80);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Adc_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);
    cpu.LDX_immediate(0x03);

    var cycles = cpu.ADC_zero_page_x(0xFF);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Adc_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);

    var cycles = cpu.ADC_absolute(0x1234);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Adc_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.ADC_absolute_x(0x2000);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Adc_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.ADC_absolute_x(0x20FF);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Adc_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x3008, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);
    cpu.LDY_immediate(0x08);

    var cycles = cpu.ADC_absolute_y(0x3000);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Adc_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x3101, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);
    cpu.LDY_immediate(0x02);

    var cycles = cpu.ADC_absolute_y(0x30FF);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Adc_IndexedIndirect_AppliesXOffsetAndReadsIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    bus.WriteByte(0x1234, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);
    cpu.LDX_immediate(0x04);

    var cycles = cpu.ADC_indexed_indirect(0x20);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Adc_IndexedIndirect_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0000, 0x00);
    bus.WriteByte(0x0001, 0x40);
    bus.WriteByte(0x4000, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);
    cpu.LDX_immediate(0x01);

    var cycles = cpu.ADC_indexed_indirect(0xFF);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Adc_IndirectIndexed_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5004, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);
    cpu.LDY_immediate(0x04);

    var cycles = cpu.ADC_indirect_indexed(0x40);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Adc_IndirectIndexed_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x34);
    bus.WriteByte(0x0000, 0x12);
    bus.WriteByte(0x1234, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);

    var cycles = cpu.ADC_indirect_indexed(0xFF);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Adc_IndirectIndexed_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0xFF);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5101, 0x15);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x10);
    cpu.LDY_immediate(0x02);

    var cycles = cpu.ADC_indirect_indexed(0x40);

    cpu.A.Should().Be(0x25);
    cycles.Should().Be(6);
  }
}
