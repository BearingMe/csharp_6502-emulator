using mos6502.src.Domain.Entities;
using mos6502.src.Domain.Enums;
using mos6502.src.Domain.Objects;

namespace mos6502.Domain.Tests;

public class RegistersTests
{
  private readonly Cpu cpu = new(new Bus());

  [Fact]
  public void Registers_StartWithExpectedValues()
  {
    Assert.Equal(0xFFFC, cpu.ProgramCounter.Value);
    Assert.Equal(0x00FD, cpu.StackPointer.Value);

    Assert.Equal(0x00, cpu.Accumulator.Value);
    Assert.Equal(0x00, cpu.XRegister.Value);
    Assert.Equal(0x00, cpu.YRegister.Value);
  }

  [Fact]
  public void Flags_StartWithInterruptSet()
  {
    Assert.True(cpu.Flags.HasFlag(Status.Interrupt));
  }

  [Fact]
  public void Flags_CanBeSet()
  {
    cpu.Flags |= Status.Carry;

    Assert.True(cpu.Flags.HasFlag(Status.Carry));
  }

  [Fact]
  public void Flags_CanBeCleared()
  {
    cpu.Flags |= Status.Carry;
    cpu.Flags &= ~Status.Carry;

    Assert.False(cpu.Flags.HasFlag(Status.Carry));
  }

  [Fact]
  public void Flags_CanHoldMultipleFlags()
  {
    cpu.Flags |= Status.Carry | Status.Zero | Status.Negative;

    Assert.True(cpu.Flags.HasFlag(Status.Carry));
    Assert.True(cpu.Flags.HasFlag(Status.Zero));
    Assert.True(cpu.Flags.HasFlag(Status.Negative));
    Assert.True(cpu.Flags.HasFlag(Status.Interrupt));
  }

  [Fact]
  public void ClearingOneFlag_DoesNotClearOthers()
  {
    cpu.Flags |= Status.Carry | Status.Zero;
    cpu.Flags &= ~Status.Carry;

    Assert.False(cpu.Flags.HasFlag(Status.Carry));
    Assert.True(cpu.Flags.HasFlag(Status.Zero));
  }
}