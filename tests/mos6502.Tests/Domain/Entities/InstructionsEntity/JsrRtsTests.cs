namespace Mos6502.Tests.Domain.Entities;

public class JsrRtsTests
{
  [Fact]
  public void Jsr_PushesReturnAddressMinusOneAndJumpsToTargetAddress()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x8003; // PC points past the 3-byte JSR instruction (0x8000 + 3)

    var cycles = cpu.JsrAbsolute(0x9000);

    cpu.PC.Should().Be(0x9000);
    // Return address pushed is PC - 1 = 0x8002
    bus.ReadByte(0x01FD).Should().Be(0x80); // High byte
    bus.ReadByte(0x01FC).Should().Be(0x02); // Low byte
    cpu.StackPointer.Should().Be(0xFB);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Rts_PullsAddressAndIncrementsByOneToSetProgramCounter()
  {
    var bus = new Bus();
    // Simulate return address 0x8002 pushed by JSR
    bus.WriteByte(0x01FD, 0x80);
    bus.WriteByte(0x01FC, 0x02);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.StackPointer = 0xFB;

    var cycles = cpu.Rts();

    // Pulled 0x8002, PC becomes 0x8002 + 1 = 0x8003
    cpu.PC.Should().Be(0x8003);
    cpu.StackPointer.Should().Be(0xFD);
    cycles.Should().Be(6);
  }

  [Fact]
  public void JsrAndRts_RoundTrip_RestoresOriginalProgramCounter()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x8003; // After fetching 3-byte JSR $9050 from 0x8000

    cpu.JsrAbsolute(0x9050);
    cpu.PC.Should().Be(0x9050);

    cpu.Rts();
    cpu.PC.Should().Be(0x8003);
    cpu.StackPointer.Should().Be(0xFD);
  }
}
