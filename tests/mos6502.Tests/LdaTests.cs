namespace mos6502.Tests;

public class LdaTests
{
  [Theory]
  [InlineData(0x00, true, false)]
  [InlineData(0x42, false, false)]
  [InlineData(0x80, false, true)]
  [InlineData(0xFF, false, true)]
  public void Lda_LoadsValueAndUpdatesFlags(
    u8 value,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    cpu.LDA_immediate(value);

    cpu.A.Should().Be(value);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
  }

  [Fact]
  public void Lda_PreservesUnrelatedFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    cpu.LDA_immediate(0x42);

    cpu.Status.Should().Be(Status.Interrupt);
  }

  [Fact]
  public void Lda_Immediate_ReadsOperandAndReturnsTwoCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    var cycles = cpu.LDA_immediate(0x37);

    cpu.A.Should().Be(0x37);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Lda_ZeroPage_ReadsFromAddressAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x55);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDA_zero_page(0x42);

    cpu.A.Should().Be(0x55);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Lda_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0080, 0x66);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDA_zero_page_x(0x80);

    cpu.A.Should().Be(0x66);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Lda_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x77);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDA_zero_page_x(0xFF);

    cpu.A.Should().Be(0x77);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Lda_Absolute_ReadsFromAddressAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x88);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDA_absolute(0x1234);

    cpu.A.Should().Be(0x88);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Lda_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0x99);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDA_absolute_x(0x2000);

    cpu.A.Should().Be(0x99);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Lda_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x3000, 0xBB);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDA_absolute_y(0x3000);

    cpu.A.Should().Be(0xBB);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Lda_IndexedIndirect_AppliesXOffsetAndReadsIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0020, 0x34);
    bus.WriteByte(0x0021, 0x12);
    bus.WriteByte(0x1234, 0xDD);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDA_indexed_indirect(0x20);

    cpu.A.Should().Be(0xDD);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Lda_IndexedIndirect_WrapsHighByteAtEndOfZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x34);
    bus.WriteByte(0x0000, 0x12);
    bus.WriteByte(0x1234, 0xEF);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDA_indexed_indirect(0xFF);

    cpu.A.Should().Be(0xEF);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Lda_IndirectIndexed_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5000, 0xF1);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDA_indirect_indexed(0x40);

    cpu.A.Should().Be(0xF1);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Lda_IndirectIndexed_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x34);
    bus.WriteByte(0x0000, 0x12);
    bus.WriteByte(0x1234, 0xF3);
    var cpu = new Emulator(bus);

    var cycles = cpu.LDA_indirect_indexed(0xFF);

    cpu.A.Should().Be(0xF3);
    cycles.Should().Be(5);
  }
}