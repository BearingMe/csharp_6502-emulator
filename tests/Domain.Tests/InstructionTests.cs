using mos6502.src.Domain.Entities;
using mos6502.src.Domain.Enums;
using mos6502.src.Domain.Objects;

namespace mos6502.Domain.Tests;

public class InstructionTests
{
  private readonly Bus bus = new();
  private readonly Cpu cpu;

  public InstructionTests()
  {
    cpu = new(bus);
  }

  [Theory]
  [InlineData(0x10, 0x20, 0x30, false, false, false, false)]
  [InlineData(0xFF, 0x01, 0x00, true, true, false, false)]
  [InlineData(0x80, 0x80, 0x00, true, true, true, false)]
  [InlineData(0x7F, 0x01, 0x80, false, false, true, true)]
  [InlineData(0x00, 0x00, 0x00, false, true, false, false)]
  public void ADC_ProducesExpectedResultAndFlags(
      byte accumulator,
      byte operand,
      byte expectedResult,
      bool expectedCarry,
      bool expectedZero,
      bool expectedOverflow,
      bool expectedNegative)
  {
    cpu.Accumulator = accumulator;
    cpu.Flags = Status.Interrupt;

    cpu.ADC(
        AddressingMode.Immediate,
        operand
    );

    Assert.Equal(expectedResult, cpu.Accumulator.Value);
    Assert.Equal(expectedCarry, cpu.Flags.HasFlag(Status.Carry));
    Assert.Equal(expectedZero, cpu.Flags.HasFlag(Status.Zero));
    Assert.Equal(expectedOverflow, cpu.Flags.HasFlag(Status.Overflow));
    Assert.Equal(expectedNegative, cpu.Flags.HasFlag(Status.Negative));
  }

  [Theory]
  [InlineData(0xFF, 0x0F, 0x0F, false, false)]
  [InlineData(0xF0, 0x0F, 0x00, true, false)]
  [InlineData(0x80, 0xFF, 0x80, false, true)]
  [InlineData(0x7F, 0x80, 0x00, true, false)]
  [InlineData(0xAA, 0x55, 0x00, true, false)]
  [InlineData(0xFE, 0x81, 0x80, false, true)]
  [InlineData(0x00, 0x00, 0x00, true, false)]
  public void AND_ProducesExpectedResultAndFlags(
      byte accumulator,
      byte operand,
      byte expectedResult,
      bool expectedZero,
      bool expectedNegative)
  {
    cpu.Accumulator = accumulator;
    cpu.Flags = Status.Interrupt;

    cpu.AND(
        AddressingMode.Immediate,
        operand
    );

    Assert.Equal(expectedResult, cpu.Accumulator.Value);
    Assert.Equal(expectedZero, cpu.Flags.HasFlag(Status.Zero));
    Assert.Equal(expectedNegative, cpu.Flags.HasFlag(Status.Negative));
  }
}