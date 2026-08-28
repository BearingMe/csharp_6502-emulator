namespace Mos6502.Tests.Domain.Entities;

public class StxTests
{
  [Fact]
  public void Stx_ZeroPage_WritesXToMemory()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x42);

    var cycles = cpu.StxZeroPage(0x10);

    bus.ReadByte(0x0010).Should().Be(0x42);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Stx_ZeroPageY_AppliesYOffsetAndWritesToMemory()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x42);
    cpu.LdyImmediate(0x05);

    var cycles = cpu.StxZeroPageY(0x10);

    bus.ReadByte(0x0015).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Stx_ZeroPageY_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x42);
    cpu.LdyImmediate(0x02);

    var cycles = cpu.StxZeroPageY(0xFF);

    bus.ReadByte(0x0001).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Stx_Absolute_WritesXToMemory()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x42);

    var cycles = cpu.StxAbsolute(0x1234);

    bus.ReadByte(0x1234).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Stx_DoesNotModifyFlags()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdxImmediate(0x42);
    cpu.Status = Status.Carry | Status.Zero | Status.Negative;
    var flagsBefore = cpu.Status;

    cpu.StxAbsolute(0x1234);

    cpu.Status.Should().Be(flagsBefore);
  }
}
