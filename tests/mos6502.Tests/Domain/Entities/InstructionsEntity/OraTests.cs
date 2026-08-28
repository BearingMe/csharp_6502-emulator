namespace Mos6502.Tests.Domain.Entities;

public class OraTests
{
  [Theory]
  [InlineData(0xF0, 0x0F, 0xFF, false, true)]
  [InlineData(0x00, 0x00, 0x00, true, false)]
  [InlineData(0x00, 0x7F, 0x7F, false, false)]
  public void Ora_PerformsBitwiseOr_AndUpdatesFlags(
    u8 initialA,
    u8 operand,
    u8 expectedA,
    bool zero,
    bool negative)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(initialA);

    var cycles = cpu.OraImmediate(operand);

    cpu.A.Should().Be(expectedA);
    cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
    cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Ora_ZeroPage_ReadsOperandAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xF0);

    var cycles = cpu.OraZeroPage(0x42);

    cpu.A.Should().Be(0xFF);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Ora_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x0085, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xF0);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.OraZeroPageX(0x80);

    cpu.A.Should().Be(0xFF);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ora_Absolute_ReadsOperandAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xF0);

    var cycles = cpu.OraAbsolute(0x1234);

    cpu.A.Should().Be(0xFF);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ora_AbsoluteX_AppliesXOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x2004, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xF0);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.OraAbsoluteX(0x2000);

    cpu.A.Should().Be(0xFF);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ora_AbsoluteX_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x2104, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xF0);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.OraAbsoluteX(0x20FF);

    cpu.A.Should().Be(0xFF);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Ora_AbsoluteY_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x3008, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xF0);
    cpu.LdyImmediate(0x08);

    var cycles = cpu.OraAbsoluteY(0x3000);

    cpu.A.Should().Be(0xFF);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Ora_AbsoluteY_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x3101, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xF0);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.OraAbsoluteY(0x30FF);

    cpu.A.Should().Be(0xFF);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Ora_IndexedIndirect_AppliesXOffsetAndReadsIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    bus.WriteByte(0x1234, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xF0);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.OraIndexedIndirect(0x20);

    cpu.A.Should().Be(0xFF);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Ora_IndirectIndexed_AppliesYOffsetWithoutPageCrossing()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5004, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xF0);
    cpu.LdyImmediate(0x04);

    var cycles = cpu.OraIndirectIndexed(0x40);

    cpu.A.Should().Be(0xFF);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Ora_IndirectIndexed_AddsCycleWhenPageIsCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0xFF);
    bus.WriteByte(0x0041, 0x50);
    bus.WriteByte(0x5101, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xF0);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.OraIndirectIndexed(0x40);

    cpu.A.Should().Be(0xFF);
    cycles.Should().Be(6);
  }
}
