using FluentAssertions;
using mos6502.src.Application;
using mos6502.src.Domain.Entities;
using mos6502.src.Domain.Enums;
using mos6502.src.Domain.Objects;
using mos6502.Tests.Helpers;

namespace mos6502.Tests.Unit;

public class CpuTests
{
    [Fact]
    public void Cpu_Constructor_InitializesRegistersAndMemoryCorrectly()
    {
        // Arrange & Act
        var bus = new Bus();
        var cpu = new Cpu(bus);

        // Assert
        cpu.GetA().Should().Be(0);
        cpu.GetX().Should().Be(0);
        cpu.GetY().Should().Be(0);
        cpu.GetPc().Should().Be(0xFFFC);
        cpu.GetSp().Should().Be(0x00FD);
        cpu.GetFlags().Should().Be(Status.Interrupt);
        cpu.GetMemory().Should().HaveCount(0x10000);
    }

    [Fact]
    public void Emulator_Reset_ResetsFlagsPcAndRegisters()
    {
        // Arrange
        var emulator = new Emulator();
        emulator.Cpu.SetA(0x55);
        emulator.Cpu.SetX(0xAA);
        emulator.Cpu.SetY(0x33);
        emulator.Cpu.SetPc(0x1000);
        emulator.Cpu.SetSp(0x0150);
        emulator.Cpu.SetFlags(Status.Zero | Status.Negative);

        // Act
        emulator.Reset();

        // Assert
        emulator.Cpu.GetFlags().Should().Be(Status.Interrupt);
        emulator.Cpu.GetPc().Should().Be(0xFFFC);
        emulator.Cpu.GetSp().Should().Be(0x00FD);
        emulator.Cpu.GetA().Should().Be(0);
        emulator.Cpu.GetX().Should().Be(0);
        emulator.Cpu.GetY().Should().Be(0);
    }

    [Theory]
    [InlineData(0xEA)] // NOP
    [InlineData(0x00)] // BRK
    [InlineData(0xFF)] // Invalid
    public void Step_UnknownOpcode_ThrowsException(byte opcode)
    {
        // Arrange
        var emulator = new Emulator();
        emulator.Cpu.SetPc(0x0200);
        emulator.Bus.Write(new Unassigned16Bits(0x0200), new Unassigned8Bits(opcode));

        // Act
        Action act = () => emulator.Step();

        // Assert
        act.Should().Throw<Exception>()
           .WithMessage($"Unknown opcode: {opcode:X2}");
    }
}
