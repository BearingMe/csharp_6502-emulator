namespace mos6502.Tests;

public class E2E_003_InterruptLifecycle
{
  [Fact]
  public void InterruptLifecycle_ExecutesBrkIsrRtiAndResumesCorrectly()
  {
    var bus = new Bus();
    // Reset vector -> $8000
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    // IRQ/BRK vector -> $9000
    bus.WriteByte(0xFFFE, 0x00);
    bus.WriteByte(0xFFFF, 0x90);

    var cpu = new Emulator(bus);

    // Main program at $8000: BRK, NOP
    byte[] mainProgram =
    [
      0x00, // BRK ($8000)
      0xEA  // NOP ($8001) - skipped padding on standard 6502 BRK
    ];

    // ISR at $9000: LDX #$42, RTI
    byte[] isrProgram =
    [
      0xA2, 0x42, // LDX #$42 ($9000)
      0x40        // RTI      ($9002)
    ];

    cpu.LoadRom(mainProgram, 0x8000);
    cpu.LoadRom(isrProgram, 0x9000);
    cpu.Reset();

    var initialSp = cpu.StackPointer; // 0xFA
    var initialStatus = cpu.Status;    // Status.Interrupt

    // Step 1: BRK at $8000
    var brkCycles = cpu.Step();
    cpu.PC.Should().Be(0x9000);
    cpu.Status.HasFlag(Status.Interrupt).Should().BeTrue();
    cpu.StackPointer.Should().Be((u8)(initialSp - 3));

    // Pushed return address is PC + 1 = $8002 ($80 High, $02 Low)
    bus.ReadByte((u16)(0x0100 | initialSp)).Should().Be(0x80);
    bus.ReadByte((u16)(0x0100 | (initialSp - 1))).Should().Be(0x02);
    // Pushed status has Break and Unused bits set
    bus.ReadByte((u16)(0x0100 | (initialSp - 2))).Should().Be((u8)(initialStatus | Status.Break | Status.Unused));
    brkCycles.Should().Be(7);

    // Step 2: LDX #$42 (inside ISR at $9000)
    var ldxCycles = cpu.Step();
    cpu.X.Should().Be(0x42);
    cpu.PC.Should().Be(0x9002);
    ldxCycles.Should().Be(2);

    // Step 3: RTI (inside ISR at $9002)
    var rtiCycles = cpu.Step();
    cpu.PC.Should().Be(0x8002);
    cpu.StackPointer.Should().Be(initialSp);
    // RTI clears Break bit on restored status
    cpu.Status.HasFlag(Status.Break).Should().BeFalse();
    rtiCycles.Should().Be(6);
  }
}
