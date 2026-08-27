namespace mos6502.Tests;

public class SystemTests
{
  [Fact]
  public void Nop_DoesNotModifyRegistersOrFlagsAndReturnsTwoCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x42);
    cpu.LDX_immediate(0x10);
    cpu.LDY_immediate(0x20);

    var flagsBefore = cpu.Status;
    var cycles = cpu.NOP();

    cpu.A.Should().Be(0x42);
    cpu.X.Should().Be(0x10);
    cpu.Y.Should().Be(0x20);
    cpu.Status.Should().Be(flagsBefore);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Brk_PushesReturnAddressAndStatusWithBreakBit_SetsInterruptFlagAndFetchesVector_ReturningSevenCycles()
  {
    var bus = new Bus();
    // Interrupt vector at 0xFFFE / 0xFFFF points to ISR at 0x9000
    bus.WriteByte(0xFFFE, 0x00);
    bus.WriteByte(0xFFFF, 0x90);
    var cpu = new Emulator(bus);
    cpu.JMP_absolute(0x8001); // Instruction address was 0x8000, opcode fetch advanced PC to 0x8001

    var cycles = cpu.BRK();

    // BRK pushes PC + 1 = 0x8002 to stack
    bus.ReadByte(0x01FD).Should().Be(0x80); // High byte
    bus.ReadByte(0x01FC).Should().Be(0x02); // Low byte
    // Pushed status has Break (bit 4) and Unused (bit 5) set
    bus.ReadByte(0x01FB).Should().Be((u8)(Status.Interrupt | Status.Break | Status.Unused));

    cpu.StackPointer.Should().Be(0xFA);
    cpu.Status.HasFlag(Status.Interrupt).Should().BeTrue();
    cpu.PC.Should().Be(0x9000);
    cycles.Should().Be(7);
  }
}
