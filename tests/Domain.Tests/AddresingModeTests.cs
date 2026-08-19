using mos6502.src.Domain.Entities;
using mos6502.src.Domain.Enums;
using mos6502.src.Domain.Objects;

namespace mos6502.Domain.Tests;

public class AddressingModeTests
{
  private readonly Bus bus = new();
  private readonly Cpu cpu;

  public AddressingModeTests()
  {
    cpu = new(bus);
  }

  [Fact]
  public void Immediate_ReturnsOperand()
  {
    var result = cpu.ReadOperand(
        AddressingMode.Immediate,
        (Unassigned16Bits)0x42
    );

    Assert.Equal(0x42, result.Value);
  }

  [Fact]
  public void Accumulator_ReturnsAccumulator()
  {
    cpu.Accumulator = 0x42;

    var result = cpu.ReadOperand(
        AddressingMode.Accumulator,
        0
    );

    Assert.Equal(0x42, result.Value);
  }

  [Fact]
  public void ZeroPage_ReadsFromAddress()
  {
    bus.Write(0x42, 0xAB);

    var result = cpu.ReadOperand(
        AddressingMode.ZeroPage,
        0x42
    );

    Assert.Equal(0xAB, result.Value);
  }

  [Fact]
  public void ZeroPageX_WrapsAtZeroPageBoundary()
  {
    cpu.XRegister = 0x02;
    bus.Write(0x01, 0xAB);

    var result = cpu.ReadOperand(
        AddressingMode.ZeroPageX,
        0xFF
    );

    Assert.Equal(0xAB, result.Value);
  }

  [Fact]
  public void ZeroPageY_WrapsAtZeroPageBoundary()
  {
    cpu.YRegister = 0x02;
    bus.Write(0x01, 0xAB);

    var result = cpu.ReadOperand(
        AddressingMode.ZeroPageY,
        0xFF
    );

    Assert.Equal(0xAB, result.Value);
  }

  [Fact]
  public void Absolute_ReadsFromAddress()
  {
    bus.Write(0x1234, 0xAB);

    var result = cpu.ReadOperand(
        AddressingMode.Absolute,
        0x1234
    );

    Assert.Equal(0xAB, result.Value);
  }

  [Fact]
  public void AbsoluteX_AddsXRegister()
  {
    cpu.XRegister = 0x10;
    bus.Write(0x1244, 0xAB);

    var result = cpu.ReadOperand(
        AddressingMode.AbsoluteX,
        0x1234
    );

    Assert.Equal(0xAB, result.Value);
  }

  [Fact]
  public void AbsoluteY_AddsYRegister()
  {
    cpu.YRegister = 0x10;
    bus.Write(0x1244, 0xAB);

    var result = cpu.ReadOperand(
        AddressingMode.AbsoluteY,
        0x1234
    );

    Assert.Equal(0xAB, result.Value);
  }

  [Fact]
  public void Relative_AddsOffsetToProgramCounter()
  {
    cpu.ProgramCounter = 0x1000;
    bus.Write(0x1010, 0xAB);

    var result = cpu.ReadOperand(
        AddressingMode.Relative,
        0x10
    );

    Assert.Equal(0xAB, result.Value);
  }

  [Fact]
  public void IndexedIndirect_WrapsPointerInZeroPage()
  {
    cpu.XRegister = 0x01;

    bus.Write(0x00, 0x34);
    bus.Write(0x01, 0x12);
    bus.Write(0x1234, 0xAB);

    var result = cpu.ReadOperand(
        AddressingMode.IndexedIndirect,
        0xFF
    );

    Assert.Equal(0xAB, result.Value);
  }

  [Fact]
  public void IndirectIndexed_AddsYRegisterToPointer()
  {
    cpu.YRegister = 0x10;

    bus.Write(0x42, 0x34);
    bus.Write(0x43, 0x12);
    bus.Write(0x1244, 0xAB);

    var result = cpu.ReadOperand(
        AddressingMode.IndirectIndexed,
        0x42
    );

    Assert.Equal(0xAB, result.Value);
  }
}