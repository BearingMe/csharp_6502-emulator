using mos6502.src.Domain.Entities;
using mos6502.src.Domain.Enums;

namespace mos6502.Domain.Tests;

public class RegistersTests
{
  private readonly Registers registers = new();

  [Fact]
  public void Registers_StartWithExpectedValues()
  {
    Assert.Equal(0xFFFC, registers.PC);
    Assert.Equal(0x00FD, registers.Stkp);

    Assert.Equal(0x00, registers.Acc);
    Assert.Equal(0x00, registers.X);
    Assert.Equal(0x00, registers.Y);
    Assert.Equal(Status.Interrupt, registers.Status);
  }

  [Fact]
  public void Flags_StartWithInterruptSet()
  {
    Assert.True(registers.IsFlag(Status.Interrupt));
  }

  [Fact]
  public void Flags_CanBeSet()
  {
    var regs = registers;
    regs.SetFlag(Status.Carry, true);

    Assert.True(regs.IsFlag(Status.Carry));
  }

  [Fact]
  public void Flags_CanBeCleared()
  {
    var regs = registers;
    regs.SetFlag(Status.Carry, true);
    regs.SetFlag(Status.Carry, false);

    Assert.False(regs.IsFlag(Status.Carry));
  }

  [Fact]
  public void Flags_CanHoldMultipleFlags()
  {
    var regs = registers;
    regs.SetFlag(Status.Carry, true);
    regs.SetFlag(Status.Zero, true);
    regs.SetFlag(Status.Negative, true);

    Assert.True(regs.IsFlag(Status.Carry));
    Assert.True(regs.IsFlag(Status.Zero));
    Assert.True(regs.IsFlag(Status.Negative));
    Assert.True(regs.IsFlag(Status.Interrupt));
  }

  [Fact]
  public void ClearingOneFlag_DoesNotClearOthers()
  {
    var regs = registers;
    regs.SetFlag(Status.Carry, true);
    regs.SetFlag(Status.Zero, true);
    regs.SetFlag(Status.Carry, false);

    Assert.False(regs.IsFlag(Status.Carry));
    Assert.True(regs.IsFlag(Status.Zero));
  }
}