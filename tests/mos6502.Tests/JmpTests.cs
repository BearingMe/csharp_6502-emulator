namespace mos6502.Tests;

public class JmpTests
{
  [Fact]
  public void Jmp_Absolute_SetsProgramCounterAndReturnsThreeCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    var cycles = cpu.JMP_absolute(0x8050);

    cpu.PC.Should().Be(0x8050);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Jmp_Absolute_PreservesStatusFlags()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0xFF);
    cpu.ADC_immediate(0x80); // sets Carry and Negative flags

    var flagsBefore = cpu.Status;
    cpu.JMP_absolute(0x1234);

    cpu.Status.Should().Be(flagsBefore);
  }

  [Fact]
  public void Jmp_Indirect_Standard_SetsProgramCounterAndReturnsFiveCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1000, 0x50);
    bus.WriteByte(0x1001, 0x80);
    var cpu = new Emulator(bus);

    var cycles = cpu.JMP_indirect(0x1000);

    cpu.PC.Should().Be(0x8050);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Jmp_Indirect_PageBoundaryBug_FetchesHighByteFromSamePageAndReturnsFiveCycles()
  {
    var bus = new Bus();
    // Replicating NMOS page-wrap bug on $10FF:
    // Low byte from $10FF, High byte from $1000 (NOT $1100)
    bus.WriteByte(0x10FF, 0x80);
    bus.WriteByte(0x1000, 0x40);
    bus.WriteByte(0x1100, 0x99); // Correct CMOS byte, should be ignored by NMOS
    var cpu = new Emulator(bus);

    var cycles = cpu.JMP_indirect(0x10FF);

    cpu.PC.Should().Be(0x4080);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Jmp_Indirect_PageBoundaryBug_AtMemoryCeiling_FetchesHighByteFromFF00AndReturnsFiveCycles()
  {
    var bus = new Bus();
    // Replicating NMOS page-wrap bug on $FFFF:
    // Low byte from $FFFF, High byte from $FF00 (NOT $0000)
    bus.WriteByte(0xFFFF, 0x12);
    bus.WriteByte(0xFF00, 0x34);
    var cpu = new Emulator(bus);

    var cycles = cpu.JMP_indirect(0xFFFF);

    cpu.PC.Should().Be(0x3412);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Jmp_Indirect_PreservesStatusFlags()
  {
    var bus = new Bus();
    bus.WriteByte(0x2000, 0x00);
    bus.WriteByte(0x2001, 0x30);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0xFF);
    cpu.ADC_immediate(0x80); // sets Carry and Negative flags

    var flagsBefore = cpu.Status;
    cpu.JMP_indirect(0x2000);

    cpu.Status.Should().Be(flagsBefore);
  }
}
