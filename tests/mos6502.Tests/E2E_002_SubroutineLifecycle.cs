namespace mos6502.Tests;

public class E2E_002_SubroutineLifecycle
{
  [Fact]
  public void SubroutineLifecycle_ExecutesJsrInxRtsAndResumesCorrectly()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    var cpu = new Emulator(bus);

    byte[] program =
    [
      0x20, 0x05, 0x80, // JSR $8005 ($8000)
      0xEA,             // NOP       ($8003)
      0x00,             // BRK       ($8004)
      0xE8,             // INX       ($8005)
      0x60              // RTS       ($8006)
    ];

    cpu.LoadRom(program, 0x8000);
    cpu.Reset();

    var initialSp = cpu.StackPointer; // 0xFA after Reset()

    // Step 1: JSR $8005
    var jsrCycles = cpu.Step();
    cpu.PC.Should().Be(0x8005);
    cpu.StackPointer.Should().Be((u8)(initialSp - 2));
    // JSR pushes return address minus 1 ($8002 = High $80, Low $02)
    bus.ReadByte((u16)(0x0100 | initialSp)).Should().Be(0x80);
    bus.ReadByte((u16)(0x0100 | (initialSp - 1))).Should().Be(0x02);
    jsrCycles.Should().Be(6);

    // Step 2: INX (inside subroutine)
    var inxCycles = cpu.Step();
    cpu.X.Should().Be(0x01);
    cpu.PC.Should().Be(0x8006);
    inxCycles.Should().Be(2);

    // Step 3: RTS
    var rtsCycles = cpu.Step();
    cpu.PC.Should().Be(0x8003); // $8002 + 1 = $8003 (NOP)
    cpu.StackPointer.Should().Be(initialSp);
    rtsCycles.Should().Be(6);

    // Step 4: NOP (resumed execution)
    var nopCycles = cpu.Step();
    cpu.PC.Should().Be(0x8004);
    nopCycles.Should().Be(2);
  }
}
