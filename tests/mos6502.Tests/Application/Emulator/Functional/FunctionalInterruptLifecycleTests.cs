namespace Mos6502.Tests.Application.Emulator.Functional;

public class FunctionalInterruptLifecycleTests
{
  [Fact]
  public void InterruptLifecycle_ExecutesCorrectly()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0xFFFE, 0x00);
    bus.WriteByte(0xFFFF, 0x90);

    var emulator = new Mos6502.Application.Emulator(bus);

    byte[] mainProgram =
    [
      0xA9, 0x42, // 8000: LDA #$42
      0x00,       // 8002: BRK
      0xAA        // 8003: TAX (post-interrupt target)
    ];

    byte[] isrProgram =
    [
      0xE8, // 9000: INX
      0x40  // 9001: RTI
    ];

    emulator.LoadRom(mainProgram, 0x8000);
    emulator.LoadRom(isrProgram, 0x9000);
    // Initial emulator startup has SP = 0xFD

    var step1Cycles = emulator.Step(); // LDA #$42
    emulator.A.Should().Be(0x42);
    emulator.PC.Should().Be(0x8002);
    step1Cycles.Should().Be(2);

    var step2Cycles = emulator.Step(); // BRK
    emulator.PC.Should().Be(0x9000);
    emulator.StackPointer.Should().Be(0xFA);
    emulator.Status.HasFlag(Status.Interrupt).Should().BeTrue();
    step2Cycles.Should().Be(7);

    var step3Cycles = emulator.Step(); // INX
    emulator.X.Should().Be(0x01);
    emulator.PC.Should().Be(0x9001);
    step3Cycles.Should().Be(2);

    var step4Cycles = emulator.Step(); // RTI
    emulator.PC.Should().Be(0x8004); // BRK pushes PC+1 (0x8003+1 = 0x8004)
    emulator.StackPointer.Should().Be(0xFD);
    step4Cycles.Should().Be(6);

    var totalCycles = step1Cycles + step2Cycles + step3Cycles + step4Cycles;
    totalCycles.Should().Be(17);
  }
}
