namespace Mos6502.Tests.Domain.Entities;

public class StaTests
{
  [Fact]
  public void Sta_ZeroPage_WritesAccumulatorToMemory()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);

    var cycles = cpu.StaZeroPage(0x10);

    bus.ReadByte(0x0010).Should().Be(0x42);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Sta_ZeroPageX_AppliesXOffsetAndWritesToMemory()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.StaZeroPageX(0x10);

    bus.ReadByte(0x0015).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sta_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);
    cpu.LdxImmediate(0x02);

    var cycles = cpu.StaZeroPageX(0xFF);

    bus.ReadByte(0x0001).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sta_Absolute_WritesAccumulatorToMemory()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);

    var cycles = cpu.StaAbsolute(0x1234);

    bus.ReadByte(0x1234).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sta_AbsoluteX_AppliesXOffsetAndWritesToMemory()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.StaAbsoluteX(0x2000);

    bus.ReadByte(0x2004).Should().Be(0x42);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sta_AbsoluteX_HasFixedFiveCyclesEvenWhenPageCrossed()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.StaAbsoluteX(0x20FF);

    bus.ReadByte(0x2104).Should().Be(0x42);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sta_AbsoluteY_AppliesYOffsetAndWritesToMemory()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);
    cpu.LdyImmediate(0x08);

    var cycles = cpu.StaAbsoluteY(0x3000);

    bus.ReadByte(0x3008).Should().Be(0x42);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sta_AbsoluteY_HasFixedFiveCyclesEvenWhenPageCrossed()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.StaAbsoluteY(0x30FF);

    bus.ReadByte(0x3101).Should().Be(0x42);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Sta_IndexedIndirect_WritesToIndirectAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);
    cpu.LdxImmediate(0x04);

    var cycles = cpu.StaIndexedIndirect(0x20);

    bus.ReadByte(0x1234).Should().Be(0x42);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Sta_IndirectIndexed_WritesToIndirectIndexedAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0x00);
    bus.WriteByte(0x0041, 0x50);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);
    cpu.LdyImmediate(0x04);

    var cycles = cpu.StaIndirectIndexed(0x40);

    bus.ReadByte(0x5004).Should().Be(0x42);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Sta_IndirectIndexed_HasFixedSixCyclesEvenWhenPageCrossed()
  {
    var bus = new Bus();
    bus.WriteByte(0x0040, 0xFF);
    bus.WriteByte(0x0041, 0x50);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.StaIndirectIndexed(0x40);

    bus.ReadByte(0x5101).Should().Be(0x42);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Sta_DoesNotModifyFlags()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);
    cpu.Status = Status.Carry | Status.Zero | Status.Negative;
    var flagsBefore = cpu.Status;

    cpu.StaAbsolute(0x1234);

    cpu.Status.Should().Be(flagsBefore);
  }
}
