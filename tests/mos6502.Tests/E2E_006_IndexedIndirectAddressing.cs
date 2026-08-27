namespace mos6502.Tests;

public class E2E_006_IndexedIndirectAddressing
{
  [Fact]
  public void IndexedIndirect_LdaAndSta_ResolvesPointerThroughZeroPageX()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    // Setup zero page pointer at ($20 + X = $24):
    // $24 = $34 (low), $25 = $12 (high) -> Target address $1234
    bus.WriteByte(0x0024, 0x34);
    bus.WriteByte(0x0025, 0x12);
    bus.WriteByte(0x1234, 0x77); // Source value to load

    // Setup zero page pointer at ($40 + X = $44):
    // $44 = $00 (low), $45 = $03 (high) -> Target address $0300
    bus.WriteByte(0x0044, 0x00);
    bus.WriteByte(0x0045, 0x03);

    var cpu = new Emulator(bus);

    // Program:
    // $8000: A2 04    LDX #$04
    // $8002: A1 20    LDA ($20,X) -> reads from pointer at $24 ($1234) -> A = $77
    // $8004: 81 40    STA ($40,X) -> writes to pointer at $44 ($0300)
    byte[] program =
    [
      0xA2, 0x04, // LDX #$04
      0xA1, 0x20, // LDA ($20,X)
      0x81, 0x40  // STA ($40,X)
    ];

    cpu.LoadRom(program, 0x8000);
    cpu.Reset();

    var ldxCycles = cpu.Step();
    ldxCycles.Should().Be(2);
    cpu.X.Should().Be(0x04);
    cpu.PC.Should().Be(0x8002);

    var ldaCycles = cpu.Step();
    ldaCycles.Should().Be(6); // 2 (base) + 4 (addressing)
    cpu.A.Should().Be(0x77);
    cpu.PC.Should().Be(0x8004);

    var staCycles = cpu.Step();
    staCycles.Should().Be(6); // 2 (base) + 4 (addressing)
    bus.ReadByte(0x0300).Should().Be(0x77);
    cpu.PC.Should().Be(0x8006);
  }

  [Fact]
  public void IndirectIndexed_LdaAndSta_ResolvesPointerAndAddsYWithPageCrossCheck()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    // Setup pointer at ZP $30:
    // $30 = $F0, $31 = $10 -> base pointer $10F0
    // With Y = $20, target address = $10F0 + $20 = $1110 (Page crossed: $10 vs $11)
    bus.WriteByte(0x0030, 0xF0);
    bus.WriteByte(0x0031, 0x10);
    bus.WriteByte(0x1110, 0x88); // Source value to load

    // Setup pointer at ZP $50:
    // $50 = $00, $51 = $04 -> base pointer $0400
    // With Y = $20, target address = $0420 (Same page)
    bus.WriteByte(0x0050, 0x00);
    bus.WriteByte(0x0051, 0x04);

    var cpu = new Emulator(bus);

    // Program:
    // $8000: A0 20    LDY #$20
    // $8002: B1 30    LDA ($30),Y -> target $1110 (page crossed: 5 + 1 = 6 cycles)
    // $8004: 91 50    STA ($50),Y -> target $0420 (same page: 2 base + 3 addressing = 5 cycles)
    byte[] program =
    [
      0xA0, 0x20, // LDY #$20
      0xB1, 0x30, // LDA ($30),Y
      0x91, 0x50  // STA ($50),Y
    ];

    cpu.LoadRom(program, 0x8000);
    cpu.Reset();

    var ldyCycles = cpu.Step();
    ldyCycles.Should().Be(2);
    cpu.Y.Should().Be(0x20);

    var ldaCycles = cpu.Step();
    // 2 (base) + 3 (addressing) + 1 (page crossed) = 6 cycles
    ldaCycles.Should().Be(6);
    cpu.A.Should().Be(0x88);
    cpu.PC.Should().Be(0x8004);

    var staCycles = cpu.Step();
    // 2 (base) + 3 (addressing) = 5 cycles
    staCycles.Should().Be(5);
    bus.ReadByte(0x0420).Should().Be(0x88);
    cpu.PC.Should().Be(0x8006);
  }
}
