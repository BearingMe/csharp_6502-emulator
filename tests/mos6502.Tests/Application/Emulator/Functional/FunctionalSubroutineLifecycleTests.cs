namespace Mos6502.Tests.Application.Emulator.Functional;

public class FunctionalSubroutineLifecycleTests
{
  [Fact]
  public void SubroutineLifecycle_ExecutesCorrectly()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    var emulator = new Mos6502.Application.Emulator(bus);

    byte[] program =
    [
      0xA9, 0x01,             // 8000: LDA #$01
      0x20, 0x10, 0x80,       // 8002: JSR $8010
      0xAA,                   // 8005: TAX
      0x00,                   // 8006: BRK (halt)
      0xEA, 0xEA, 0xEA, 0xEA, // 8007-800A: padding
      0xEA, 0xEA, 0xEA, 0xEA, // 800B-800E: padding
      0xEA,                   // 800F: padding
      0x69, 0x05,             // 8010: ADC #$05
      0x60                    // 8012: RTS
    ];

    emulator.LoadRom(program, 0x8000);
    // Initial emulator startup has SP = 0xFD

    var step1Cycles = emulator.Step(); // LDA #$01
    emulator.A.Should().Be(0x01);
    emulator.PC.Should().Be(0x8002);
    step1Cycles.Should().Be(2);

    var step2Cycles = emulator.Step(); // JSR $8010
    emulator.PC.Should().Be(0x8010);
    emulator.StackPointer.Should().Be(0xFB);
    bus.ReadByte(0x01FD).Should().Be(0x80);
    bus.ReadByte(0x01FC).Should().Be(0x04);
    step2Cycles.Should().Be(6);

    var step3Cycles = emulator.Step(); // ADC #$05
    emulator.A.Should().Be(0x06);
    emulator.PC.Should().Be(0x8012);
    step3Cycles.Should().Be(2);

    var step4Cycles = emulator.Step(); // RTS
    emulator.PC.Should().Be(0x8005);
    emulator.StackPointer.Should().Be(0xFD);
    step4Cycles.Should().Be(6);

    var step5Cycles = emulator.Step(); // TAX
    emulator.X.Should().Be(0x06);
    emulator.PC.Should().Be(0x8006);
    step5Cycles.Should().Be(2);

    var totalCycles = step1Cycles + step2Cycles + step3Cycles + step4Cycles + step5Cycles;
    totalCycles.Should().Be(18);
  }
}
