namespace mos6502.Tests;

public class E2E_007_IndirectJmpPageWrap
{
  [Fact]
  public void IndirectJmp_WhenPointerIsAtEndOfPage_FetchesHighByteFromSamePage()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    // Replicate NMOS 6502 $xxFF bug:
    // JMP ($30FF)
    // Low byte of target read from $30FF: $50
    // High byte of target read from $3000 (wrapped inside page $30, NOT $3100): $90
    // CMOS byte at $3100: $22 (must NOT be used)
    bus.WriteByte(0x30FF, 0x50);
    bus.WriteByte(0x3000, 0x90);
    bus.WriteByte(0x3100, 0x22);

    // Target instruction at $9050: LDX #$99
    bus.WriteByte(0x9050, 0xA2);
    bus.WriteByte(0x9051, 0x99);

    var cpu = new Emulator(bus);

    // Program at $8000:
    // $8000: 6C FF 30    JMP ($30FF)
    byte[] program =
    [
      0x6C, 0xFF, 0x30 // JMP ($30FF)
    ];

    cpu.LoadRom(program, 0x8000);
    cpu.Reset();

    var jmpCycles = cpu.Step();
    jmpCycles.Should().Be(5);
    cpu.PC.Should().Be(0x9050);

    // Execute target instruction to confirm machine continuation
    var ldxCycles = cpu.Step();
    ldxCycles.Should().Be(2);
    cpu.X.Should().Be(0x99);
    cpu.PC.Should().Be(0x9052);
  }

  [Fact]
  public void IndirectJmp_StandardPointer_JumpsToTargetAddress()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    // Standard pointer at $2000:
    // $2000 = $20, $2001 = $40 -> Target address $4020
    bus.WriteByte(0x2000, 0x20);
    bus.WriteByte(0x2001, 0x40);

    // Target instruction at $4020: LDA #$12
    bus.WriteByte(0x4020, 0xA9);
    bus.WriteByte(0x4021, 0x12);

    var cpu = new Emulator(bus);

    byte[] program =
    [
      0x6C, 0x00, 0x20 // JMP ($2000)
    ];

    cpu.LoadRom(program, 0x8000);
    cpu.Reset();

    var jmpCycles = cpu.Step();
    jmpCycles.Should().Be(5);
    cpu.PC.Should().Be(0x4020);

    var ldaCycles = cpu.Step();
    ldaCycles.Should().Be(2);
    cpu.A.Should().Be(0x12);
    cpu.PC.Should().Be(0x4022);
  }
}
