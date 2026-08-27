namespace mos6502.Tests;

public class RtiTests
{
  [Fact]
  public void Rti_PullsStatusAndProgramCounterFromStack_AndReturnsSixCycles()
  {
    var cpu = new Cpu();
    var emulator = new Emulator(cpu);

    // Prepare stack frame simulating interrupt push:
    // Pushed order was: PC_hi, PC_lo, Status (with B and U bits)
    // Pull order in RTI is: Status, PC_lo, PC_hi
    emulator.LDX_immediate(0xFA);
    emulator.TXS();

    cpu.WriteByte(0x01FB, (u8)(Status.Carry | Status.Zero | Status.Negative | Status.Break | Status.Unused));
    cpu.WriteByte(0x01FC, 0x34); // PC Low
    cpu.WriteByte(0x01FD, 0x12); // PC High

    var cycles = emulator.RTI();

    emulator.PC.Should().Be(0x1234);
    emulator.StackPointer.Should().Be(0xFD);
    emulator.Status.HasFlag(Status.Carry).Should().BeTrue();
    emulator.Status.HasFlag(Status.Zero).Should().BeTrue();
    emulator.Status.HasFlag(Status.Negative).Should().BeTrue();
    emulator.Status.HasFlag(Status.Break).Should().BeFalse();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Rti_RestoresAllConditionFlags_WhileIgnoringBreakAndUnusedBits()
  {
    var cpu = new Cpu();
    var emulator = new Emulator(cpu);

    emulator.LDX_immediate(0xFA);
    emulator.TXS();

    // Push full byte 0xFF (all flags set)
    cpu.WriteByte(0x01FB, 0xFF);
    cpu.WriteByte(0x01FC, 0x00);
    cpu.WriteByte(0x01FD, 0x80);

    emulator.RTI();

    emulator.Status.HasFlag(Status.Carry).Should().BeTrue();
    emulator.Status.HasFlag(Status.Zero).Should().BeTrue();
    emulator.Status.HasFlag(Status.Interrupt).Should().BeTrue();
    emulator.Status.HasFlag(Status.Decimal).Should().BeTrue();
    emulator.Status.HasFlag(Status.Overflow).Should().BeTrue();
    emulator.Status.HasFlag(Status.Negative).Should().BeTrue();
    emulator.Status.HasFlag(Status.Break).Should().BeFalse();
  }

  [Fact]
  public void Rti_DoesNotModifyAccumulatorOrIndexRegisters()
  {
    var cpu = new Cpu();
    var emulator = new Emulator(cpu);

    emulator.LDA_immediate(0x42);
    emulator.LDX_immediate(0xFA);
    emulator.TXS();
    emulator.LDY_immediate(0x24);

    cpu.WriteByte(0x01FB, (u8)Status.Interrupt);
    cpu.WriteByte(0x01FC, 0x50);
    cpu.WriteByte(0x01FD, 0x90);

    emulator.RTI();

    emulator.A.Should().Be(0x42);
    emulator.Y.Should().Be(0x24);
    emulator.PC.Should().Be(0x9050);
  }

  [Fact]
  public void Rti_WhenStackPointerWrapsAroundPageBoundary_PullsCorrectBytesAndUpdatesStackPointer()
  {
    var cpu = new Cpu();
    var emulator = new Emulator(cpu);

    // Set SP to 0xFE so:
    // Pull 1 (Status): SP -> 0xFF, reads 0x01FF
    // Pull 2 (PC low): SP -> 0x00, reads 0x0100
    // Pull 3 (PC high): SP -> 0x01, reads 0x0101
    emulator.LDX_immediate(0xFE);
    emulator.TXS();

    cpu.WriteByte(0x01FF, (u8)Status.Carry);
    cpu.WriteByte(0x0100, 0xEF); // PC Low
    cpu.WriteByte(0x0101, 0xBE); // PC High

    var cycles = emulator.RTI();

    emulator.PC.Should().Be(0xBEEF);
    emulator.StackPointer.Should().Be(0x01);
    emulator.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(6);
  }
}
