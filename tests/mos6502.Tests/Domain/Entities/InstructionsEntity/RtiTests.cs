namespace Mos6502.Tests.Domain.Entities;

public class RtiTests
{
  [Fact]
  public void Rti_PullsStatusAndProgramCounterFromStack()
  {
    var bus = new Bus();
    // Simulate stack pushed by interrupt at SP = 0xFD
    // Stack contains: PCH at 0x01FD, PCL at 0x01FC, Status at 0x01FB
    bus.WriteByte(0x01FD, 0x80); // PCH
    bus.WriteByte(0x01FC, 0x02); // PCL
    bus.WriteByte(0x01FB, (u8)(Status.Carry | Status.Zero)); // Status without Break/Unused
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.StackPointer = 0xFA;
    cpu.Status = Status.Interrupt;

    var cycles = cpu.Rti();

    cpu.PC.Should().Be(0x8002);
    cpu.StackPointer.Should().Be(0xFD);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.Status.HasFlag(Status.Interrupt).Should().BeFalse();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Rti_PreservesBreakAndUnusedStatusBitsFromCurrentCpu()
  {
    var bus = new Bus();
    bus.WriteByte(0x01FD, 0x90);
    bus.WriteByte(0x01FC, 0x00);
    // Incoming status has Break and Unused set on stack, but they should be masked
    bus.WriteByte(0x01FB, (u8)(Status.Carry | Status.Break | Status.Unused));
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.StackPointer = 0xFA;
    // Current CPU has no Break or Unused set
    cpu.Status = 0;

    cpu.Rti();

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    // Break bit (bit 4) and Unused bit (bit 5) in CPU status register should remain clear
    cpu.Status.HasFlag(Status.Break).Should().BeFalse();
    cpu.Status.HasFlag(Status.Unused).Should().BeFalse();
  }

  [Fact]
  public void BrkAndRti_RoundTrip_RestoresCpuState()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFE, 0x00);
    bus.WriteByte(0xFFFF, 0x90); // ISR at 0x9000
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x8001; // Instruction address was 0x8000, opcode fetch advanced PC to 0x8001
    cpu.Status = Status.Carry | Status.Decimal;

    // BRK pushes PC + 1 (0x8002) and Status
    cpu.Brk();
    cpu.PC.Should().Be(0x9000);
    cpu.StackPointer.Should().Be(0xFA);

    // RTI in ISR should restore PC to 0x8002 and flags to Carry | Decimal
    cpu.Rti();
    cpu.PC.Should().Be(0x8002);
    cpu.StackPointer.Should().Be(0xFD);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Decimal).Should().BeTrue();
  }
}
