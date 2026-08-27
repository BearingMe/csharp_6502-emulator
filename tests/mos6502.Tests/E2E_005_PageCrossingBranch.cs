namespace mos6502.Tests;

public class E2E_005_PageCrossingBranch
{
  [Fact]
  public void Branch_WhenTakenAndCrossesPageBoundary_ConsumesFourCycles()
  {
    var bus = new Bus();
    // Program loaded at $80FE:
    // $80FE: A9 01    LDA #$01 (clears Zero flag)
    // $8100: D0 05    BNE +$05 (PC when Branch() executes is $8102 [page $81], wait:
    // If branch instruction is at $80FE:
    //   Fetch opcode D0 at $80FE -> PC becomes $80FF
    //   Fetch operand 05 at $80FF -> PC becomes $8100 (in page $81)
    //   Branch taken -> PC becomes $8100 + 5 = $8105 (in page $81)
    //   HasPageCrossed($8100, $8105) -> false! Both are in page $81.
    //
    // To cross page forward:
    // Branch instruction at $80FC:
    //   Fetch opcode D0 at $80FC -> PC becomes $80FD
    //   Fetch operand 05 at $80FD -> PC becomes $80FE (in page $80)
    //   Branch taken -> new PC = $80FE + 5 = $8103 (in page $81)
    //   HasPageCrossed($80FE, $8103) -> true ($80 vs $81)!
    bus.WriteByte(0xFFFC, 0xFA);
    bus.WriteByte(0xFFFD, 0x80);

    var cpu = new Emulator(bus);

    byte[] program =
    [
      0xA9, 0x01, // LDA #$01 ($80FA)
      0xEA,       // NOP      ($80FC)
      0xD0, 0x05  // BNE +5   ($80FD) -> PC after fetch is $80FF ($80), target is $80FF + 5 = $8104 ($81)
    ];

    cpu.LoadRom(program, 0x80FA);
    cpu.Reset();

    var ldaCycles = cpu.Step();
    ldaCycles.Should().Be(2);
    cpu.PC.Should().Be(0x80FC);

    var nopCycles = cpu.Step();
    nopCycles.Should().Be(2);
    cpu.PC.Should().Be(0x80FD);

    var bneCycles = cpu.Step();
    // Taken branch (3) + Page crossed (1) = 4 cycles
    bneCycles.Should().Be(4);
    cpu.PC.Should().Be(0x8104);
  }

  [Fact]
  public void Branch_BackwardWhenTakenAndCrossesPageBoundary_ConsumesFourCycles()
  {
    var bus = new Bus();
    // Program at $8100:
    // $8100: A9 01    LDA #$01 (clears Zero flag)
    // $8102: D0 F0    BNE -$10 (PC after operand fetch is $8104 [page $81], target is $8104 - 16 = $80F4 [page $80])
    // Origin PC $8104 is in page $81, target $80F4 is in page $80 -> backward page crossed!
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x81);

    var cpu = new Emulator(bus);

    byte[] program =
    [
      0xA9, 0x01, // LDA #$01 ($8100)
      0xD0, 0xF0  // BNE -16  ($8102) -> target $8104 - 16 = $80F4
    ];

    cpu.LoadRom(program, 0x8100);
    cpu.Reset();

    cpu.Step();
    var bneCycles = cpu.Step();

    bneCycles.Should().Be(4);
    cpu.PC.Should().Be(0x80F4);
  }
}
