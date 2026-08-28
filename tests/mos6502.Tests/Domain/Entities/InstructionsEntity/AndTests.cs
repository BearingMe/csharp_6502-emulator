namespace Mos6502.Tests.Domain.Entities;

public class AndTests
{
  [Theory]
  [InlineData(0xFF, 0x0F, 0x0F, false, false)]
  [InlineData(0xF0, 0x0F, 0x00, true, false)]
  [InlineData(0xFF, 0x80, 0x80, false, true)]
  public void And_PerformsBitwiseAnd_AndUpdatesFlags(
    u8 initialA,
    u8 operand,
    u8 expectedA,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(initialA);

    var cycles = cpu.AndImmediate(operand);

    cpu.A.Should().Be(expectedA);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void And_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);

    var cycles = cpu.AndZeroPage(0x42);

    cpu.A.Should().Be(0x0F);
    cycles.Should().Be(3);
  }

  [Fact]
  public void And_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.AndZeroPageX(0x80);

    cpu.A.Should().Be(0x0F);
    cycles.Should().Be(4);
  }

  [Fact]
  public void And_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);

    var cycles = cpu.AndAbsolute(0x1234);

    cpu.A.Should().Be(0x0F);
    cycles.Should().Be(4);
  }

  [Fact]
  public void And_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.AndAbsoluteX(0x2000);

    cpu.A.Should().Be(0x0F);
    cycles.Should().Be(4);
  }

  [Fact]
  public void And_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.AndAbsoluteX(0x20FF);

    cpu.A.Should().Be(0x0F);
    cycles.Should().Be(5);
  }

  [Fact]
  public void And_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x3008, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdyImmediate(0x08);

    var cycles = cpu.AndAbsoluteY(0x3000);

    cpu.A.Should().Be(0x0F);
    cycles.Should().Be(4);
  }

  [Fact]
  public void And_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x3101, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.AndAbsoluteY(0x30FF);

    cpu.A.Should().Be(0x0F);
    cycles.Should().Be(5);
  }

  [Fact]
  public void And_IndexedIndirect_AppliesXOffsetAndReadsIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    bus.WriteByte(0x1234, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.AndIndexedIndirect(0x20);

    cpu.A.Should().Be(0x0F);
    cycles.Should().Be(6);
  }

  [Fact]
  public void And_IndirectIndexed_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5004, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdyImmediate(0x04);

    var cycles = cpu.AndIndirectIndexed(0x40);

    cpu.A.Should().Be(0x0F);
    cycles.Should().Be(5);
  }

  [Fact]
  public void And_IndirectIndexed_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0xFF);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5101, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xFF);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.AndIndirectIndexed(0x40);

    cpu.A.Should().Be(0x0F);
    cycles.Should().Be(6);
  }
}
