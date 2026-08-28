namespace Mos6502.Tests.Domain.Entities;

public class StyTests
{
  [Fact]
  public void Sty_ZeroPage_WritesYToMemory()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x42);

    var cycles = cpu.StyZeroPage(0x10);

    bus.ReadByte(0x0010).Should().Be(0x42);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Sty_ZeroPageX_AppliesXOffsetAndWritesToMemory()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x42);
    cpu.LdxImmediate(0x05);

    var cycles = cpu.StyZeroPageX(0x10);

    bus.ReadByte(0x0015).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sty_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x42);
    cpu.LdxImmediate(0x02);

    var cycles = cpu.StyZeroPageX(0xFF);

    bus.ReadByte(0x0001).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sty_Absolute_WritesYToMemory()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x42);

    var cycles = cpu.StyAbsolute(0x1234);

    bus.ReadByte(0x1234).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sty_DoesNotModifyFlags()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdyImmediate(0x42);
    cpu.Status = Status.Carry | Status.Zero | Status.Negative;
    var flagsBefore = cpu.Status;

    cpu.StyAbsolute(0x1234);

    cpu.Status.Should().Be(flagsBefore);
  }
}
