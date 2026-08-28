namespace Mos6502.Tests.Application.Emulator.Functional;

public class FunctionalBranchingLoopTests
{
  [Fact]
  public void BranchingLoop_CountsDownAndTerminates()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    var emulator = new Mos6502.Application.Emulator(bus);

    byte[] program =
    [
      0xA2, 0x03, // 8000: LDX #$03
      0xCA,       // 8002: DEX
      0xD0, 0xFD  // 8003: BNE -3 -> targets 8002 (PC=8005 + -3 = 8002)
    ];

    emulator.LoadRom(program, 0x8000);
    emulator.Reset();

    var ldxCycles = emulator.Step(); // LDX #$03
    emulator.X.Should().Be(0x03);
    emulator.PC.Should().Be(0x8002);
    ldxCycles.Should().Be(2);

    // Iteration 1: X=3 -> 2, branch taken (forward 0 bytes crossing? No, backward on same page)
    var dex1Cycles = emulator.Step(); // DEX -> X=2
    emulator.X.Should().Be(0x02);
    dex1Cycles.Should().Be(2);

    var bne1Cycles = emulator.Step(); // BNE taken -> 8002
    emulator.PC.Should().Be(0x8002);
    bne1Cycles.Should().Be(3);

    // Iteration 2: X=2 -> 1, branch taken
    var dex2Cycles = emulator.Step(); // DEX -> X=1
    emulator.X.Should().Be(0x01);
    dex2Cycles.Should().Be(2);

    var bne2Cycles = emulator.Step(); // BNE taken -> 8002
    emulator.PC.Should().Be(0x8002);
    bne2Cycles.Should().Be(3);

    // Iteration 3: X=1 -> 0, branch not taken
    var dex3Cycles = emulator.Step(); // DEX -> X=0
    emulator.X.Should().Be(0x00);
    emulator.Status.HasFlag(Status.Zero).Should().BeTrue();
    dex3Cycles.Should().Be(2);

    var bne3Cycles = emulator.Step(); // BNE not taken -> PC=8005
    emulator.PC.Should().Be(0x8005);
    bne3Cycles.Should().Be(2);

    var totalCycles = ldxCycles + dex1Cycles + bne1Cycles + dex2Cycles + bne2Cycles + dex3Cycles + bne3Cycles;
    totalCycles.Should().Be(16);
  }
}
