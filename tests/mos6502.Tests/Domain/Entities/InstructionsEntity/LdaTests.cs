namespace Mos6502.Tests.Domain.Entities;

public class LdaTests
{
  [Theory]
  [InlineData(0x05, false, false)]
  [InlineData(0x00, true, false)]
  [InlineData(0x80, false, true)]
  public void Lda_Immediate_SetsAccumulatorAndFlags(
    u8 operand,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LdaImmediate(operand);

    cpu.A.Should().Be(operand);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Lda_ZeroPage_ReadsMemoryAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x55);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LdaZeroPage(0x42);

    cpu.A.Should().Be(0x55);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Lda_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x33);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.LdaZeroPageX(0x80);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Lda_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x77);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x03);

    var cycles = cpu.LdaZeroPageX(0xFF);

    cpu.A.Should().Be(0x77);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Lda_Absolute_ReadsMemoryAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x99);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LdaAbsolute(0x1234);

    cpu.A.Should().Be(0x99);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Lda_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x11);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.LdaAbsoluteX(0x2000);

    cpu.A.Should().Be(0x11);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Lda_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x22);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.LdaAbsoluteX(0x20FF);

    cpu.A.Should().Be(0x22);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Lda_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x3008, 0x33);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x08);

    var cycles = cpu.LdaAbsoluteY(0x3000);

    cpu.A.Should().Be(0x33);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Lda_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x3101, 0x44);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.LdaAbsoluteY(0x30FF);

    cpu.A.Should().Be(0x44);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Lda_IndexedIndirect_AppliesXOffsetAndReadsIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    bus.WriteByte(0x1234, 0x66);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.LdaIndexedIndirect(0x20);

    cpu.A.Should().Be(0x66);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Lda_IndexedIndirect_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0000, 0x00);
    bus.WriteByte(0x0001, 0x40);
    bus.WriteByte(0x4000, 0x77);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x01);

    var cycles = cpu.LdaIndexedIndirect(0xFF);

    cpu.A.Should().Be(0x77);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Lda_IndirectIndexed_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5004, 0x88);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x04);

    var cycles = cpu.LdaIndirectIndexed(0x40);

    cpu.A.Should().Be(0x88);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Lda_IndirectIndexed_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x34);
    bus.WriteByte(0x0000, 0x12);
    bus.WriteByte(0x1234, 0x99);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.LdaIndirectIndexed(0xFF);

    cpu.A.Should().Be(0x99);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Lda_IndirectIndexed_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0xFF);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5101, 0xAA);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.LdaIndirectIndexed(0x40);

    cpu.A.Should().Be(0xAA);
    cycles.Should().Be(6);
  }
}
