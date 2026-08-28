namespace Mos6502.Tests.Domain.Entities;

public class AddressingTests
{
  [Fact]
  public void Immediate_ReturnsValueWithZeroCycles()
  {
    var cpu = new Mos6502.Domain.Entities.Cpu();
    var bus = new Mos6502.Domain.Entities.Bus();
    var addressing = new Mos6502.Domain.Entities.Addressing(cpu, bus);

    var result = addressing.Immediate(0x42);

    result.Value.Should().Be(0x42);
    result.Cycles.Should().Be(0);
  }

  [Fact]
  public void ZeroPage_ReturnsAddressWithOneCycle()
  {
    var cpu = new Mos6502.Domain.Entities.Cpu();
    var bus = new Mos6502.Domain.Entities.Bus();
    var addressing = new Mos6502.Domain.Entities.Addressing(cpu, bus);

    var result = addressing.ZeroPage(0x80);

    result.Value.Should().Be(0x80);
    result.Cycles.Should().Be(1);
  }

  [Fact]
  public void ZeroPageX_WrapsWithinZeroPage()
  {
    var cpu = new Mos6502.Domain.Entities.Cpu { X = 0x05 };
    var bus = new Mos6502.Domain.Entities.Bus();
    var addressing = new Mos6502.Domain.Entities.Addressing(cpu, bus);

    var result = addressing.ZeroPageX(0xFD);

    result.Value.Should().Be(0x02);
    result.Cycles.Should().Be(2);
  }

  [Fact]
  public void ZeroPageY_WrapsWithinZeroPage()
  {
    var cpu = new Mos6502.Domain.Entities.Cpu { Y = 0x05 };
    var bus = new Mos6502.Domain.Entities.Bus();
    var addressing = new Mos6502.Domain.Entities.Addressing(cpu, bus);

    var result = addressing.ZeroPageY(0xFD);

    result.Value.Should().Be(0x02);
    result.Cycles.Should().Be(2);
  }

  [Fact]
  public void AbsoluteX_AddsExtraCycleOnPageCross()
  {
    var cpu = new Mos6502.Domain.Entities.Cpu { X = 0x02 };
    var bus = new Mos6502.Domain.Entities.Bus();
    var addressing = new Mos6502.Domain.Entities.Addressing(cpu, bus);

    var result = addressing.AbsoluteX(0x20FF);

    result.Value.Should().Be(0x2101);
    result.Cycles.Should().Be(3);
  }

  [Fact]
  public void AbsoluteY_AddsExtraCycleOnPageCross()
  {
    var cpu = new Mos6502.Domain.Entities.Cpu { Y = 0x02 };
    var bus = new Mos6502.Domain.Entities.Bus();
    var addressing = new Mos6502.Domain.Entities.Addressing(cpu, bus);

    var result = addressing.AbsoluteY(0x30FF);

    result.Value.Should().Be(0x3101);
    result.Cycles.Should().Be(3);
  }

  [Fact]
  public void Indirect_ReplicatesNMOSPageWrapBug()
  {
    var cpu = new Mos6502.Domain.Entities.Cpu();
    var bus = new Mos6502.Domain.Entities.Bus();
    bus.WriteByte(0x10FF, 0x50);
    bus.WriteByte(0x1000, 0x90);
    bus.WriteByte(0x1100, 0x22);
    var addressing = new Mos6502.Domain.Entities.Addressing(cpu, bus);

    var result = addressing.Indirect(0x10FF);

    result.Value.Should().Be(0x9050);
    result.Cycles.Should().Be(4);
  }
}
