namespace mos6502.Tests;

public class StxTests
{
  [Fact]
  public void Stx_PreservesStatusFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x00);

    cpu.STX_zero_page(0x10);

    cpu.Status.Should().Be(Status.Interrupt | Status.Zero);
  }

  [Fact]
  public void Stx_ZeroPage_WritesXRegisterToAddressAndReturnsThreeCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x42);

    var cycles = cpu.STX_zero_page(0x80);

    bus.ReadByte(0x0080).Should().Be(0x42);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Stx_ZeroPageY_AppliesYOffsetAndReturnsFourCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x42);
    cpu.LDY_immediate(0x05);

    var cycles = cpu.STX_zero_page_y(0x80);

    bus.ReadByte(0x0085).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Stx_ZeroPageY_WrapsWithinZeroPage()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x42);
    cpu.LDY_immediate(0x03);

    var cycles = cpu.STX_zero_page_y(0xFF);

    bus.ReadByte(0x0002).Should().Be(0x42);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Stx_Absolute_WritesXRegisterToAddressAndReturnsFourCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDX_immediate(0x42);

    var cycles = cpu.STX_absolute(0x1234);

    bus.ReadByte(0x1234).Should().Be(0x42);
    cycles.Should().Be(4);
  }
}
