namespace Mos6502.Tests.Domain.Entities;

public class JmpTests
{
  [Fact]
  public void Jmp_Absolute_SetsProgramCounterAndReturnsThreeCycles()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.JmpAbsolute(0x9050);

    cpu.PC.Should().Be(0x9050);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Jmp_Indirect_SetsProgramCounterAndReturnsFiveCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1000, 0x50);
    bus.WriteByte(0x1001, 0x90);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.JmpIndirect(0x1000);

    cpu.PC.Should().Be(0x9050);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Jmp_Indirect_ReplicatesNMOSPageWrapBug()
  {
    var bus = new Bus();
    bus.WriteByte(0x10FF, 0x50);
    bus.WriteByte(0x1000, 0x90);
    bus.WriteByte(0x1100, 0x22); // CMOS high byte, ignored on NMOS
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.JmpIndirect(0x10FF);

    cpu.PC.Should().Be(0x9050);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Jmp_DoesNotModifyFlags()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = Status.Carry | Status.Zero | Status.Negative;
    var flagsBefore = cpu.Status;

    cpu.JmpAbsolute(0x1234);

    cpu.Status.Should().Be(flagsBefore);
  }

  [Fact]
  public void Jmp_Indirect_DoesNotModifyFlags()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0x00);
    bus.WriteByte(0x2001, 0x80);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = Status.Interrupt | Status.Decimal;
    var flagsBefore = cpu.Status;

    cpu.JmpIndirect(0x2000);

    cpu.Status.Should().Be(flagsBefore);
  }

  [Fact]
  public void Jmp_Indirect_ReadsVectorFromZeroPage()
  {
    var bus = new Bus();
    bus.WriteByte(0x0020, 0x78);
    bus.WriteByte(0x0021, 0x56);
    var cpu = new Mos6502.Application.Emulator(bus);

    var cycles = cpu.JmpIndirect(0x0020);

    cpu.PC.Should().Be(0x5678);
    cycles.Should().Be(5);
  }
}
