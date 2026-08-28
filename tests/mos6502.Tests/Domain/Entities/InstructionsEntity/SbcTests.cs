namespace Mos6502.Tests.Domain.Entities;

public class SbcTests
{
  [Theory]
  [InlineData(0x50, 0xF0, 0x5F, false, false, false, false)]
  [InlineData(0x50, 0x50, 0xFF, false, false, true, false)]
  [InlineData(0x50, 0x7F, 0xD0, false, false, true, false)]
  [InlineData(0x50, 0x80, 0xCF, false, false, true, true)]
  [InlineData(0xD0, 0x70, 0x5F, true, false, false, true)]
  public void Sbc_SubtractsValues_AndUpdatesFlags_WithoutInitialCarry(
    u8 initialA,
    u8 operand,
    u8 expectedA,
    bool carry,
    bool zero,
    bool negative,
    bool overflow)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(initialA);

    var cycles = cpu.SbcImmediate(operand);

    cpu.A.Should().Be(expectedA);
    cpu.Status.HasFlag(Status.Carry).Should().Be(carry);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cpu.Status.HasFlag(Status.Overflow).Should().Be(overflow);
    cycles.Should().Be(2);
  }

  [Theory]
  [InlineData(0x50, 0x10, 0x40, true, false, false, false)]
  [InlineData(0x50, 0x50, 0x00, true, true, false, false)]
  [InlineData(0x50, 0x70, 0xE0, false, false, true, false)]
  [InlineData(0x50, 0x90, 0xC0, false, false, true, true)]
  [InlineData(0xD0, 0x70, 0x60, true, false, false, true)]
  public void Sbc_SubtractsValues_AndUpdatesFlags_WithInitialCarry(
    u8 initialA,
    u8 operand,
    u8 expectedA,
    bool carry,
    bool zero,
    bool negative,
    bool overflow)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag

    cpu.LdaImmediate(initialA);
    var cycles = cpu.SbcImmediate(operand);

    cpu.A.Should().Be(expectedA);
    cpu.Status.HasFlag(Status.Carry).Should().Be(carry);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cpu.Status.HasFlag(Status.Overflow).Should().Be(overflow);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Sbc_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);

    var cycles = cpu.SbcZeroPage(0x42);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Sbc_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.SbcZeroPageX(0x80);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sbc_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0002, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);
    cpu.LdxImmediate(0x03);

    var cycles = cpu.SbcZeroPageX(0xFF);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sbc_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);

    var cycles = cpu.SbcAbsolute(0x1234);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sbc_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.SbcAbsoluteX(0x2000);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sbc_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.SbcAbsoluteX(0x20FF);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sbc_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x3008, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);
    cpu.LdyImmediate(0x08);

    var cycles = cpu.SbcAbsoluteY(0x3000);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sbc_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x3101, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.SbcAbsoluteY(0x30FF);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sbc_IndexedIndirect_AppliesXOffsetAndReadsIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    bus.WriteByte(0x1234, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.SbcIndexedIndirect(0x20);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Sbc_IndexedIndirect_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0000, 0x00);
    bus.WriteByte(0x0001, 0x40);
    bus.WriteByte(0x4000, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);
    cpu.LdxImmediate(0x01);

    var cycles = cpu.SbcIndexedIndirect(0xFF);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Sbc_IndirectIndexed_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5004, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);
    cpu.LdyImmediate(0x04);

    var cycles = cpu.SbcIndirectIndexed(0x40);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sbc_IndirectIndexed_WrapsPointerWithinZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x00FF, 0x34);
    bus.WriteByte(0x0000, 0x12);
    bus.WriteByte(0x1234, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);

    var cycles = cpu.SbcIndirectIndexed(0xFF);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sbc_IndirectIndexed_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0xFF);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5101, 0x15);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.AdcImmediate(0x01); // sets Carry flag
    cpu.LdaImmediate(0x35);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.SbcIndirectIndexed(0x40);

    cpu.A.Should().Be(0x20);
    cycles.Should().Be(6);
  }
}
