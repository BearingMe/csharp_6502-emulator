namespace Mos6502.Tests.Application.Emulator.Functional;

public class FunctionalIndirectJmpPageWrapTests
{
  [Fact]
  public void IndirectJmpPageWrap_ReplicatesHardwarePageBoundaryBug()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    // Vector at $10FF: low byte at $10FF ($50), high byte wraps to $1000 ($90), NOT $1100 ($22)
    bus.WriteByte(0x10FF, 0x50);
    bus.WriteByte(0x1000, 0x90);
    bus.WriteByte(0x1100, 0x22); // CMOS value, ignored by NMOS

    // Target code at $9050: INX, NOP
    bus.WriteByte(0x9050, 0xE8); // INX
    bus.WriteByte(0x9051, 0xEA); // NOP

    var emulator = new Mos6502.Application.Emulator(bus);

    byte[] program =
    [
      0x6C, 0xFF, 0x10 // 8000: JMP ($10FF) -> jumps to $9050 on NMOS (buggy page wrap)
    ];

    emulator.LoadRom(program, 0x8000);
    emulator.Reset();

    var jmpCycles = emulator.Step(); // JMP ($10FF)
    jmpCycles.Should().Be(5);
    emulator.PC.Should().Be(0x9050);

    var inxCycles = emulator.Step(); // INX at target
    inxCycles.Should().Be(2);
    emulator.X.Should().Be(0x01);
    emulator.PC.Should().Be(0x9051);

    var totalCycles = jmpCycles + inxCycles;
    totalCycles.Should().Be(7);
  }
}
