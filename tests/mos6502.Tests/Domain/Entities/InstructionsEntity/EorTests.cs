namespace Mos6502.Tests.Domain.Entities;

public class EorTests
{
  [Theory]
  [InlineData(0xFF, 0x0F, 0xF0, false, true)]
  [InlineData(0xAA, 0xAA, 0x00, true, false)]
  [InlineData(0x00, 0x7F, 0x7F, false, false)]
  public void Eor_PerformsExclusiveOr_AndUpdatesFlags(
    u8 initialA,
    u8 operand,
    u8 expectedA,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(initialA);

    var cycles = cpu.EorImmediate(operand);

    cpu.A.Should().Be(expectedA);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Eor_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);

    var cycles = cpu.EorZeroPage(0x42);

    cpu.A.Should().Be(0xF0);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Eor_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.EorZeroPageX(0x80);

    cpu.A.Should().Be(0xF0);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Eor_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);

    var cycles = cpu.EorAbsolute(0x1234);

    cpu.A.Should().Be(0xF0);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Eor_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.EorAbsoluteX(0x2000);

    cpu.A.Should().Be(0xF0);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Eor_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.EorAbsoluteX(0x20FF);

    cpu.A.Should().Be(0xF0);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Eor_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x3008, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdyImmediate(0x08);

    var cycles = cpu.EorAbsoluteY(0x3000);

    cpu.A.Should().Be(0xF0);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Eor_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x3101, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.EorAbsoluteY(0x30FF);

    cpu.A.Should().Be(0xF0);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Eor_IndexedIndirect_AppliesXOffsetAndReadsIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    bus.WriteByte(0x1234, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.EorIndexedIndirect(0x20);

    cpu.A.Should().Be(0xF0);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Eor_IndirectIndexed_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5004, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdyImmediate(0x04);

    var cycles = cpu.EorIndirectIndexed(0x40);

    cpu.A.Should().Be(0xF0);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Eor_IndirectIndexed_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0xFF);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5101, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.EorIndirectIndexed(0x40);

    cpu.A.Should().Be(0xF0);
    cycles.Should().Be(6);
  }
}
