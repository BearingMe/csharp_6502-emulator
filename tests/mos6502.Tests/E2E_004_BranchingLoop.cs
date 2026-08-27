namespace mos6502.Tests;

public class E2E_004_BranchingLoop
{
  [Fact]
  public void BranchingLoop_CountsDownAndStoresResult()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    var cpu = new Emulator(bus);

    // Program:
    // $8000: A2 05    LDX #$05
    // loop ($8002):
    // $8002: CA       DEX
    // $8003: D0 FD    BNE loop (-3 from $8005 -> $8002)
    // $8005: 8E 00 02 STX $0200
    byte[] program =
    [
      0xA2, 0x05,       // LDX #$05 ($8000)
      0xCA,             // DEX      ($8002)
      0xD0, 0xFD,       // BNE $FD  ($8003) -> target $8005 + (-3) = $8002
      0x8E, 0x00, 0x02  // STX $0200 ($8005)
    ];

    cpu.LoadRom(program, 0x8000);
    cpu.Reset();

    var totalCycles = 0;

    // Step 1: LDX #$05
    var ldxCycles = cpu.Step();
    cpu.X.Should().Be(0x05);
    cpu.PC.Should().Be(0x8002);
    ldxCycles.Should().Be(2);
    totalCycles += ldxCycles;

    // 4 loop iterations where branch is TAKEN (X = 4, 3, 2, 1)
    for (var expectedX = 4; expectedX >= 1; expectedX--)
    {
      var dexCycles = cpu.Step();
      cpu.X.Should().Be((u8)expectedX);
      cpu.PC.Should().Be(0x8003);
      dexCycles.Should().Be(2);
      totalCycles += dexCycles;

      var bneCycles = cpu.Step();
      cpu.PC.Should().Be(0x8002); // branch taken back to DEX
      bneCycles.Should().Be(3);    // 3 cycles (branch taken, same page)
      totalCycles += bneCycles;
    }

    // 5th iteration: X reaches 0, branch is NOT TAKEN
    var dexFinalCycles = cpu.Step();
    cpu.X.Should().Be(0x00);
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.PC.Should().Be(0x8003);
    dexFinalCycles.Should().Be(2);
    totalCycles += dexFinalCycles;

    var bneFinalCycles = cpu.Step();
    cpu.PC.Should().Be(0x8005); // branch not taken, falls through to STX
    bneFinalCycles.Should().Be(2); // 2 cycles (branch not taken)
    totalCycles += bneFinalCycles;

    // STX $0200
    var stxCycles = cpu.Step();
    bus.ReadByte(0x0200).Should().Be(0x00);
    cpu.PC.Should().Be(0x8008);
    stxCycles.Should().Be(4);
    totalCycles += stxCycles;

    // Expected total cycles:
    // LDX: 2
    // Iterations 1-4: 4 * (2 [DEX] + 3 [BNE taken]) = 20
    // Iteration 5: 2 [DEX] + 2 [BNE not taken] = 4
    // STX: 4
    // Total = 2 + 20 + 4 + 4 = 30
    totalCycles.Should().Be(30);
  }
}
