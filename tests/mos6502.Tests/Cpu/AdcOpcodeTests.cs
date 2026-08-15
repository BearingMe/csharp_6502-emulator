using FluentAssertions;
using mos6502.src;
using mos6502.Tests.Helpers;

namespace mos6502.Tests.Unit;

public class AdcOpcodeTests
{
    [Theory]
    // 6502 Specification Hardware-Correct ADC Immediate (0x69) Scenarios:
    // Format: initialA, operand, initialCarry, expectedA, expectedCarry, expectedZero, expectedOverflow, expectedNegative
    [InlineData(0x10, 0x20, false, 0x30, false, false, false, false)] // Simple positive addition without overflow
    [InlineData(0x00, 0x00, false, 0x00, false, true,  false, false)] // Result is zero -> Z set
    [InlineData(0xFE, 0x01, true,  0x00, true,  true,  false, false)] // Carry in + Carry out -> C set, Z set
    [InlineData(0xFF, 0x01, false, 0x00, true,  true,  false, false)] // Carry out -> C set, Z set
    [InlineData(0x40, 0x40, false, 0x80, false, false, true,  true)]  // Pos (64) + Pos (64) = Neg (-128) -> Signed Overflow V=T, N=T
    [InlineData(0x50, 0x50, false, 0xA0, false, false, true,  true)]  // Pos (80) + Pos (80) = Neg (-96)  -> Signed Overflow V=T, N=T
    [InlineData(0x7F, 0x01, false, 0x80, false, false, true,  true)]  // Max Pos (127) + Pos (1) = Neg (-128) -> Signed Overflow V=T, N=T
    [InlineData(0x80, 0x80, false, 0x00, true,  true,  true,  false)] // Neg (-128) + Neg (-128) = Pos (0) -> Signed Overflow V=T, C=T, Z=T
    [InlineData(0xD0, 0x90, false, 0x60, true,  false, true,  false)] // Neg (-48) + Neg (-112) = Pos (+96) -> Signed Overflow V=T, C=T
    [InlineData(0x10, 0x20, true,  0x31, false, false, false, false)] // Simple addition with Carry in
    public void ADC_ImmediateMode_ExecutesAdditionPerHardwareSpec(
        byte initialA,
        ushort operand,
        bool initialCarry,
        byte expectedA,
        bool expectedCarry,
        bool expectedZero,
        bool expectedOverflow,
        bool expectedNegative)
    {
        // Arrange
        var cpu = new mos6502.src.Cpu();
        cpu.SetA(initialA);
        cpu.SetX(0x42);
        cpu.SetY(0x24);
        cpu.SetSp(0x00FD);
        cpu.SetPc(0x0200);

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        cpu.SetFlags(initialFlags);

        var instruction = new Instruction(0x69, operand);

        // Act
        cpu.Execute(instruction);

        // Assert 6502 Specification behavior
        cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        cpu.GetX().Should().Be(0x42);
        cpu.GetY().Should().Be(0x24);
        cpu.GetSp().Should().Be(0x00FD);
        cpu.GetPc().Should().Be(0x0200);
    }
}
