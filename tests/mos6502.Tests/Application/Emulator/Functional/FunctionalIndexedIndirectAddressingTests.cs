namespace Mos6502.Tests.Application.Emulator.Functional;

public class FunctionalIndexedIndirectAddressingTests
{
  [Fact]
  public void IndexedIndirectAddressing_ExecutesCorrectly()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    // Setup zero page pointer at ($20 + X) = ($20 + $04) = $24/$25 -> $1234
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    // Setup target memory value at $1234
    bus.WriteByte(0x1234, 0x99);

    var emulator = new Mos6502.Application.Emulator(bus);

    byte[] program =
    [
      0xA2, 0x04,       // 8000: LDX #$04
      0xA1, 0x20,       // 8002: LDA ($20,X) -> reads from ($24) -> $1234 -> $99
      0x8D, 0x00, 0x03  // 8004: STA $0300
    ];

    emulator.LoadRom(program, 0x8000);
    emulator.Reset();

    var ldxCycles = emulator.Step(); // LDX #$04
    emulator.X.Should().Be(0x04);
    ldxCycles.Should().Be(2);

    var ldaCycles = emulator.Step(); // LDA ($20,X)
    emulator.A.Should().Be(0x99);
    ldaCycles.Should().Be(6);

    var staCycles = emulator.Step(); // STA $0300
    bus.ReadByte(0x0300).Should().Be(0x99);
    staCycles.Should().Be(4);

    var totalCycles = ldxCycles + ldaCycles + staCycles;
    totalCycles.Should().Be(12);
  }
}
