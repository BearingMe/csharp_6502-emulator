namespace Mos6502.Tests.Domain.Entities;

public class StackTests
{
  [Fact]
  public void Pha_PushesAccumulatorToStack_AndDecrementsStackPointer()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x42);
    cpu.StackPointer = 0xFD;

    var cycles = cpu.Pha();

    bus.ReadByte(0x01FD).Should().Be(0x42);
    cpu.StackPointer.Should().Be(0xFC);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Pla_PullsAccumulatorFromStack_AndUpdatesFlags()
  {
    var bus = new Bus();
    bus.WriteByte(0x01FD, 0x80);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.StackPointer = 0xFC;

    var cycles = cpu.Pla();

    cpu.A.Should().Be(0x80);
    cpu.StackPointer.Should().Be(0xFD);
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cycles.Should().Be(4);
  }

  [Fact]
  public void Pla_SetsZeroFlag_WhenPulledValueIsZero()
  {
    var bus = new Bus();
    bus.WriteByte(0x01FD, 0x00);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.StackPointer = 0xFC;

    cpu.Pla();

    cpu.A.Should().Be(0x00);
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
  }

  [Fact]
  public void Php_PushesStatusWithBreakAndUnusedFlagsSet()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = Status.Carry | Status.Zero;
    cpu.StackPointer = 0xFD;

    var cycles = cpu.Php();

    var expectedPushedStatus = (u8)(Status.Carry | Status.Zero | Status.Break | Status.Unused);
    bus.ReadByte(0x01FD).Should().Be(expectedPushedStatus);
    cpu.StackPointer.Should().Be(0xFC);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Plp_PullsStatus_IgnoringBreakAndUnusedBitsFromStack()
  {
    var bus = new Bus();
    bus.WriteByte(0x01FD, 0xFF); // all bits set on stack
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.StackPointer = 0xFC;
    cpu.Status = 0; // Break and Unused are not set in CPU

    var cycles = cpu.Plp();

    cpu.StackPointer.Should().Be(0xFD);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.Status.HasFlag(Status.Interrupt).Should().BeTrue();
    cpu.Status.HasFlag(Status.Decimal).Should().BeTrue();
    cpu.Status.HasFlag(Status.Overflow).Should().BeTrue();
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
    // Break and Unused in the register should be ignored from the stack pull
    cpu.Status.HasFlag(Status.Break).Should().BeFalse();
    cpu.Status.HasFlag(Status.Unused).Should().BeFalse();
    cycles.Should().Be(4);
  }

  [Fact]
  public void Stack_PhaAndPla_RoundTrip_PreservesValue()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.StackPointer = 0xFD;
    cpu.LdaImmediate(0x99);

    cpu.Pha();
    cpu.LdaImmediate(0x00);
    cpu.Pla();

    cpu.A.Should().Be(0x99);
    cpu.StackPointer.Should().Be(0xFD);
  }
}
