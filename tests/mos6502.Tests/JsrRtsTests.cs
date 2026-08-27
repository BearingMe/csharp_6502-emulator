namespace mos6502.Tests;

public class JsrRtsTests
{
  [Fact]
  public void Jsr_PushesReturnAddressMinusOneAndJumpsToTarget_ReturningSixCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.JMP_absolute(0x8003);

    var cycles = cpu.JSR_absolute(0x9050);

    // Return address is PC - 1 = 0x8002
    bus.ReadByte(0x01FD).Should().Be(0x80); // High byte pushed first
    bus.ReadByte(0x01FC).Should().Be(0x02); // Low byte pushed second
    cpu.StackPointer.Should().Be(0xFB);
    cpu.PC.Should().Be(0x9050);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Rts_PullsReturnAddressIncrementsByOneAndSetsProgramCounter_ReturningSixCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);

    // Prepare stack with return address minus one: 0x8002
    cpu.LDX_immediate(0xFB);
    cpu.TXS();
    bus.WriteByte(0x01FC, 0x02); // Low byte
    bus.WriteByte(0x01FD, 0x80); // High byte

    var cycles = cpu.RTS();

    cpu.PC.Should().Be(0x8003);
    cpu.StackPointer.Should().Be(0xFD);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Jsr_And_Rts_RoundTrip_RestoresOriginalProgramCounter()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.JMP_absolute(0x8003);

    cpu.JSR_absolute(0xC000);
    cpu.PC.Should().Be(0xC000);

    cpu.RTS();
    cpu.PC.Should().Be(0x8003);
  }
}
