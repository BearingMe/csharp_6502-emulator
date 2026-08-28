namespace Mos6502.Tests.Application.Emulator.Functional;

public class FunctionalPageCrossingBranchTests
{
  [Fact]
  public void PageCrossingBranch_AddsExtraCycleWhenCrossingPageBoundary()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0xFA);
    bus.WriteByte(0xFFFD, 0x80);

    var emulator = new Mos6502.Application.Emulator(bus);

    byte[] program =
    [
      0xA9, 0x01, // 80FA: LDA #$01 (clear Zero)
      0xD0, 0x06  // 80FC: BNE +6 -> 80FE + 6 = 8104 (crosses from page 80 to 81)
    ];

    emulator.LoadRom(program, 0x80FA);
    emulator.Reset();

    var ldaCycles = emulator.Step();
    ldaCycles.Should().Be(2);

    var bneCycles = emulator.Step(); // Branch taken across page boundary
    bneCycles.Should().Be(4);        // 2 (base) + 1 (taken) + 1 (page crossed)
    emulator.PC.Should().Be(0x8104);
  }
}
