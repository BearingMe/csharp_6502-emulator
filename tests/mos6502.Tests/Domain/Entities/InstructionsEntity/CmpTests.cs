namespace Mos6502.Tests.Domain.Entities;

public class CmpTests
{
  [Theory]
  [InlineData(0x20, 0x10, true, false, false)] // A > M -> C=1, Z=0, N=0
  [InlineData(0x10, 0x20, false, false, true)] // A < M -> C=0, Z=0, N=1
  [InlineData(0x10, 0x10, true, true, false)]  // A == M -> C=1, Z=1, N=0
  public void Cmp_ComparesAccumulatorAndMemory_AndSetsFlags(
    u8 a,
    u8 operand,
    bool carry,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(a);

    var cycles = cpu.CmpImmediate(operand);

    cpu.Status.HasFlag(Status.Carry).Should().Be(carry);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Cmp_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x20);

    var cycles = cpu.CmpZeroPage(0x42);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(3);
  }

  [Fact]
  public void Cmp_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x20);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.CmpZeroPageX(0x80);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(4);
  }

  [Fact]
  public void Cmp_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x20);

    var cycles = cpu.CmpAbsolute(0x1234);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(4);
  }

  [Fact]
  public void Cmp_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x20);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.CmpAbsoluteX(0x2000);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(4);
  }

  [Fact]
  public void Cmp_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x20);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.CmpAbsoluteX(0x20FF);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Cmp_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x3008, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x20);
    cpu.LdyImmediate(0x08);

    var cycles = cpu.CmpAbsoluteY(0x3000);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(4);
  }

  [Fact]
  public void Cmp_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x3101, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x20);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.CmpAbsoluteY(0x30FF);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Cmp_IndexedIndirect_AppliesXOffsetAndReadsIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    bus.WriteByte(0x1234, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x20);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.CmpIndexedIndirect(0x20);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Cmp_IndirectIndexed_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5004, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x20);
    cpu.LdyImmediate(0x04);

    var cycles = cpu.CmpIndirectIndexed(0x40);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(5);
  }

  [Fact]
  public void Cmp_IndirectIndexed_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0xFF);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5101, 0x10);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x20);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.CmpIndirectIndexed(0x40);

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(6);
  }
}
