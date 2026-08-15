using FluentAssertions;
using mos6502.src;
using mos6502.Tests.Helpers;

namespace mos6502.Tests.Unit;

public class CpuTests
{
    [Fact]
    public void Cpu_Constructor_InitializesRegistersAndMemoryCorrectly()
    {
        // Arrange & Act
        var cpu = new mos6502.src.Cpu();

        // Assert
        cpu.GetA().Should().Be(0);
        cpu.GetX().Should().Be(0);
        cpu.GetY().Should().Be(0);
        cpu.GetPc().Should().Be(0xFFFC);
        cpu.GetSp().Should().Be(0x00FD);
        cpu.GetFlags().Should().Be(Status.Interrupt);
        cpu.GetMemory().Should().HaveCount(0x10000);
    }

    [Theory]
    [InlineData(0x00FD, 0x00FA)]
    [InlineData(0x00FF, 0x00FC)]
    public void Reset_ResetsFlagsPcAndDecrementsStackPointer(ushort initialSp, ushort expectedSp)
    {
        // Arrange
        var cpu = new mos6502.src.Cpu();
        cpu.SetA(0x55);
        cpu.SetX(0xAA);
        cpu.SetY(0x33);
        cpu.SetPc(0x1000);
        cpu.SetSp(initialSp);
        cpu.SetFlags(Status.Zero | Status.Negative);

        // Act
        cpu.Reset();

        // Assert
        cpu.GetFlags().Should().Be(Status.Interrupt);
        cpu.GetPc().Should().Be(0xFFFC);
        cpu.GetSp().Should().Be(expectedSp);

        // Verify non-targeted registers remain unmodified
        cpu.GetA().Should().Be(0x55);
        cpu.GetX().Should().Be(0xAA);
        cpu.GetY().Should().Be(0x33);
    }

    [Theory]
    [InlineData(0xEA)] // NOP
    [InlineData(0x00)] // BRK
    [InlineData(0xFF)] // Invalid
    public void Execute_UnknownOpcode_ThrowsException(byte opcode)
    {
        // Arrange
        var cpu = new mos6502.src.Cpu();
        var instruction = new Instruction(opcode, 0x00);

        // Act
        Action act = () => cpu.Execute(instruction);

        // Assert
        act.Should().Throw<Exception>()
           .WithMessage($"Unknown opcode: {instruction:X2}");
    }
}
