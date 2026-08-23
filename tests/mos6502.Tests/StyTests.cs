namespace mos6502.Tests;

public class StyTests
{
  [Fact]
  public void Sty_PreservesStatusFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x00);

    cpu.STY_zero_page(0x10);

    cpu.Status.Should().Be(Status.Interrupt | Status.Zero);
  }

  [Fact]
  public void Sty_ZeroPage_WritesYRegisterToAddressAndReturnsThreeCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x42);

    var cycles = cpu.STY_zero_page(0x80);

    bus.ReadByte(0x0080).Should().Be(0x42);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Sty_ZeroPageX_AppliesXOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x42);
    cpu.LDX_immediate(0x05);

    var cycles = cpu.STY_zero_page_x(0x80);

    bus.ReadByte(0x0085).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sty_ZeroPageX_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x42);
    cpu.LDX_immediate(0x03);

    var cycles = cpu.STY_zero_page_x(0xFF);

    bus.ReadByte(0x0002).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Sty_Absolute_WritesYRegisterToAddressAndReturnsFourCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDY_immediate(0x42);

    var cycles = cpu.STY_absolute(0x1234);

    bus.ReadByte(0x1234).Should().Be(0x42);
    cycles.Should().Be(4);
  }
}
