using FluentAssertions;
using mos6502.src.Application;
using mos6502.src.Domain.Enums;
using mos6502.src.Domain.Objects;
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
        var emulator = new Emulator();
        emulator.Cpu.SetA(initialA);
        emulator.Cpu.SetX(0x42);
        emulator.Cpu.SetY(0x24);
        emulator.Cpu.SetSp(0x00FD);
        emulator.Cpu.SetPc(0x0200);

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        emulator.Cpu.SetFlags(initialFlags);

        emulator.Bus.Write(new Unassigned16Bits(0x0200), new Unassigned8Bits(0x69));
        emulator.Bus.Write(new Unassigned16Bits(0x0201), new Unassigned8Bits((byte)operand));

        // Act
        emulator.Step();

        // Assert 6502 Specification behavior
        emulator.Cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        emulator.Cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        emulator.Cpu.GetX().Should().Be(0x42);
        emulator.Cpu.GetY().Should().Be(0x24);
        emulator.Cpu.GetSp().Should().Be(0x00FD);
        emulator.Cpu.GetPc().Should().Be(0x0202);
    }

    [Theory]
    // 6502 Specification Hardware-Correct ADC Zero Page (0x65) Scenarios:
    // Format: initialA, zeroPageAddr, memoryValue, initialCarry, expectedA, expectedCarry, expectedZero, expectedOverflow, expectedNegative
    [InlineData(0x10, 0x40, 0x20, false, 0x30, false, false, false, false)] // Simple positive addition without overflow
    [InlineData(0x00, 0x80, 0x00, false, 0x00, false, true,  false, false)] // Result is zero -> Z set
    [InlineData(0xFE, 0x05, 0x01, true,  0x00, true,  true,  false, false)] // Carry in + Carry out -> C set, Z set
    [InlineData(0xFF, 0x10, 0x01, false, 0x00, true,  true,  false, false)] // Carry out -> C set, Z set
    [InlineData(0x40, 0x20, 0x40, false, 0x80, false, false, true,  true)]  // Pos (64) + Pos (64) = Neg (-128) -> Signed Overflow V=T, N=T
    [InlineData(0x50, 0x30, 0x50, false, 0xA0, false, false, true,  true)]  // Pos (80) + Pos (80) = Neg (-96)  -> Signed Overflow V=T, N=T
    [InlineData(0x7F, 0x44, 0x01, false, 0x80, false, false, true,  true)]  // Max Pos (127) + Pos (1) = Neg (-128) -> Signed Overflow V=T, N=T
    [InlineData(0x80, 0xFF, 0x80, false, 0x00, true,  true,  true,  false)] // Neg (-128) + Neg (-128) = Pos (0) -> Signed Overflow V=T, C=T, Z=T
    [InlineData(0xD0, 0x00, 0x90, false, 0x60, true,  false, true,  false)] // Neg (-48) + Neg (-112) = Pos (+96) -> Signed Overflow V=T, C=T
    [InlineData(0x10, 0x7F, 0x20, true,  0x31, false, false, false, false)] // Simple addition with Carry in
    public void ADC_ZeroPageMode_ExecutesAdditionPerHardwareSpec(
        byte initialA,
        byte zeroPageAddr,
        byte memoryValue,
        bool initialCarry,
        byte expectedA,
        bool expectedCarry,
        bool expectedZero,
        bool expectedOverflow,
        bool expectedNegative)
    {
        // Arrange
        var emulator = new Emulator();
        emulator.Cpu.SetA(initialA);
        emulator.Cpu.SetX(0x42);
        emulator.Cpu.SetY(0x24);
        emulator.Cpu.SetSp(0x00FD);
        emulator.Cpu.SetPc(0x0200);

        // Populate Zero Page memory
        emulator.Bus.Write(new Unassigned16Bits(zeroPageAddr), new Unassigned8Bits(memoryValue));

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        emulator.Cpu.SetFlags(initialFlags);

        emulator.Bus.Write(new Unassigned16Bits(0x0200), new Unassigned8Bits(0x65));
        emulator.Bus.Write(new Unassigned16Bits(0x0201), new Unassigned8Bits(zeroPageAddr));

        // Act
        emulator.Step();

        // Assert 6502 Specification behavior
        emulator.Cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        emulator.Cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        emulator.Cpu.GetX().Should().Be(0x42);
        emulator.Cpu.GetY().Should().Be(0x24);
        emulator.Cpu.GetSp().Should().Be(0x00FD);
        emulator.Cpu.GetPc().Should().Be(0x0202);
        emulator.Bus.Read(new Unassigned16Bits(zeroPageAddr)).Value.Should().Be(memoryValue, "Memory at zero page address must remain unmodified");
    }

    [Theory]
    // 6502 Specification Hardware-Correct ADC Zero Page,X (0x75) Scenarios:
    // Format: initialA, zeroPageAddr, xRegister, memoryValue, initialCarry, expectedA, expectedCarry, expectedZero, expectedOverflow, expectedNegative
    [InlineData(0x10, 0x40, 0x10, 0x20, false, 0x30, false, false, false, false)] // Simple positive addition without wrap
    [InlineData(0x00, 0x80, 0xFF, 0x15, false, 0x15, false, false, false, false)] // Zero page wrap: 0x80 + 0xFF = 0x7F (in page 0, NOT 0x017F)
    [InlineData(0x00, 0xFF, 0x01, 0x42, false, 0x42, false, false, false, false)] // Zero page wrap: 0xFF + 0x01 = 0x00 (wraps to start of zero page)
    [InlineData(0xFE, 0x10, 0x05, 0x01, true,  0x00, true,  true,  false, false)] // Carry in + Carry out -> C set, Z set
    [InlineData(0xFF, 0x20, 0x02, 0x01, false, 0x00, true,  true,  false, false)] // Carry out -> C set, Z set
    [InlineData(0x40, 0x30, 0x05, 0x40, false, 0x80, false, false, true,  true)]  // Pos (64) + Pos (64) = Neg (-128) -> Signed Overflow V=T, N=T
    [InlineData(0x80, 0xE0, 0x20, 0x80, false, 0x00, true,  true,  true,  false)] // Neg (-128) + Neg (-128) = Pos (0) -> Signed Overflow V=T, C=T, Z=T (wrap: 0xE0 + 0x20 = 0x00)
    [InlineData(0xD0, 0x50, 0x10, 0x90, false, 0x60, true,  false, true,  false)] // Neg (-48) + Neg (-112) = Pos (+96) -> Signed Overflow V=T, C=T
    [InlineData(0x10, 0x7F, 0x01, 0x20, true,  0x31, false, false, false, false)] // Simple addition with Carry in
    public void ADC_ZeroPageXMode_ExecutesAdditionPerHardwareSpec(
        byte initialA,
        byte zeroPageAddr,
        byte xRegister,
        byte memoryValue,
        bool initialCarry,
        byte expectedA,
        bool expectedCarry,
        bool expectedZero,
        bool expectedOverflow,
        bool expectedNegative)
    {
        // Arrange
        var emulator = new Emulator();
        emulator.Cpu.SetA(initialA);
        emulator.Cpu.SetX(xRegister);
        emulator.Cpu.SetY(0x24);
        emulator.Cpu.SetSp(0x00FD);
        emulator.Cpu.SetPc(0x0200);

        // Effective zero-page address wrapped to 8-bit range (0x00-0xFF) per 6502 spec
        byte effectiveAddr = (byte)(zeroPageAddr + xRegister);

        // Populate target Zero Page memory and set poison value at unwrapped Page 1 address and base address to verify indexing & wrapping
        emulator.Bus.Write(new Unassigned16Bits(effectiveAddr), new Unassigned8Bits(memoryValue));
        if (effectiveAddr != zeroPageAddr)
        {
            emulator.Bus.Write(new Unassigned16Bits(zeroPageAddr), new Unassigned8Bits(0xDD)); // Poison value
        }
        ushort unwrappedAddr = (ushort)(zeroPageAddr + xRegister);
        if (unwrappedAddr > 0xFF)
        {
            emulator.Bus.Write(new Unassigned16Bits(unwrappedAddr), new Unassigned8Bits(0xEE)); // Poison value
        }

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        emulator.Cpu.SetFlags(initialFlags);

        emulator.Bus.Write(new Unassigned16Bits(0x0200), new Unassigned8Bits(0x75));
        emulator.Bus.Write(new Unassigned16Bits(0x0201), new Unassigned8Bits(zeroPageAddr));

        // Act
        emulator.Step();

        // Assert 6502 Specification behavior
        emulator.Cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        emulator.Cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        emulator.Cpu.GetX().Should().Be(xRegister, "X register must remain unmodified");
        emulator.Cpu.GetY().Should().Be(0x24);
        emulator.Cpu.GetSp().Should().Be(0x00FD);
        emulator.Cpu.GetPc().Should().Be(0x0202);
        emulator.Bus.Read(new Unassigned16Bits(effectiveAddr)).Value.Should().Be(memoryValue, "Memory at effective zero-page address must remain unmodified");
    }

    [Theory]
    // 6502 Specification Hardware-Correct ADC Absolute (0x6D) Scenarios:
    // Format: initialA, address, memoryValue, initialCarry, expectedA, expectedCarry, expectedZero, expectedOverflow, expectedNegative
    [InlineData(0x10, (ushort)0x1234, 0x20, false, 0x30, false, false, false, false)] // Simple positive addition without overflow
    [InlineData(0x00, (ushort)0x0400, 0x00, false, 0x00, false, true,  false, false)] // Result is zero -> Z set
    [InlineData(0xFE, (ushort)0x0250, 0x01, true,  0x00, true,  true,  false, false)] // Carry in + Carry out -> C set, Z set
    [InlineData(0xFF, (ushort)0x8000, 0x01, false, 0x00, true,  true,  false, false)] // Carry out -> C set, Z set
    [InlineData(0x40, (ushort)0x3000, 0x40, false, 0x80, false, false, true,  true)]  // Pos (64) + Pos (64) = Neg (-128) -> Signed Overflow V=T, N=T
    [InlineData(0x80, (ushort)0xFFFF, 0x80, false, 0x00, true,  true,  true,  false)] // Neg (-128) + Neg (-128) = Pos (0) -> Signed Overflow V=T, C=T, Z=T
    [InlineData(0xD0, (ushort)0x00FF, 0x90, false, 0x60, true,  false, true,  false)] // Neg (-48) + Neg (-112) = Pos (+96) -> Signed Overflow V=T, C=T
    [InlineData(0x10, (ushort)0x1000, 0x20, true,  0x31, false, false, false, false)] // Simple addition with Carry in
    public void ADC_AbsoluteMode_ExecutesAdditionPerHardwareSpec(
        byte initialA,
        ushort address,
        byte memoryValue,
        bool initialCarry,
        byte expectedA,
        bool expectedCarry,
        bool expectedZero,
        bool expectedOverflow,
        bool expectedNegative)
    {
        // Arrange
        var emulator = new Emulator();
        emulator.Cpu.SetA(initialA);
        emulator.Cpu.SetX(0x42);
        emulator.Cpu.SetY(0x24);
        emulator.Cpu.SetSp(0x00FD);
        emulator.Cpu.SetPc(0x0200);

        // Populate memory at 16-bit target address
        emulator.Bus.Write(new Unassigned16Bits(address), new Unassigned8Bits(memoryValue));

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        emulator.Cpu.SetFlags(initialFlags);

        emulator.Bus.Write(new Unassigned16Bits(0x0200), new Unassigned8Bits(0x6D));
        emulator.Bus.Write(new Unassigned16Bits(0x0201), new Unassigned8Bits((byte)(address & 0xFF)));
        emulator.Bus.Write(new Unassigned16Bits(0x0202), new Unassigned8Bits((byte)(address >> 8)));

        // Act
        emulator.Step();

        // Assert 6502 Specification behavior
        emulator.Cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        emulator.Cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        emulator.Cpu.GetX().Should().Be(0x42);
        emulator.Cpu.GetY().Should().Be(0x24);
        emulator.Cpu.GetSp().Should().Be(0x00FD);
        emulator.Cpu.GetPc().Should().Be(0x0203);
        emulator.Bus.Read(new Unassigned16Bits(address)).Value.Should().Be(memoryValue, "Memory at target address must remain unmodified");
    }

    [Theory]
    // 6502 Specification Hardware-Correct ADC Absolute,X (0x7D) Scenarios:
    // Format: initialA, baseAddress, xRegister, memoryValue, initialCarry, expectedA, expectedCarry, expectedZero, expectedOverflow, expectedNegative
    [InlineData(0x10, (ushort)0x2000, 0x10, 0x20, false, 0x30, false, false, false, false)] // Simple addition without page cross
    [InlineData(0x00, (ushort)0x20FF, 0x01, 0x15, false, 0x15, false, false, false, false)] // Page boundary cross: 0x20FF + 0x01 = 0x2100 (crosses to page 0x21)
    [InlineData(0x00, (ushort)0xFFFF, 0x01, 0x42, false, 0x42, false, false, false, false)] // Full 16-bit address wrap: 0xFFFF + 0x01 = 0x0000
    [InlineData(0xFE, (ushort)0x1000, 0x05, 0x01, true,  0x00, true,  true,  false, false)] // Carry in + Carry out -> C set, Z set
    [InlineData(0xFF, (ushort)0x3000, 0x02, 0x01, false, 0x00, true,  true,  false, false)] // Carry out -> C set, Z set
    [InlineData(0x40, (ushort)0x4000, 0x05, 0x40, false, 0x80, false, false, true,  true)]  // Pos (64) + Pos (64) = Neg (-128) -> Signed Overflow V=T, N=T
    [InlineData(0x80, (ushort)0x50FE, 0x02, 0x80, false, 0x00, true,  true,  true,  false)] // Neg (-128) + Neg (-128) = Pos (0) -> Signed Overflow V=T, C=T, Z=T (page cross: 0x50FE+2=0x5100)
    [InlineData(0xD0, (ushort)0x6050, 0x10, 0x90, false, 0x60, true,  false, true,  false)] // Neg (-48) + Neg (-112) = Pos (+96) -> Signed Overflow V=T, C=T
    [InlineData(0x10, (ushort)0x7F00, 0x01, 0x20, true,  0x31, false, false, false, false)] // Simple addition with Carry in
    public void ADC_AbsoluteXMode_ExecutesAdditionPerHardwareSpec(
        byte initialA,
        ushort baseAddress,
        byte xRegister,
        byte memoryValue,
        bool initialCarry,
        byte expectedA,
        bool expectedCarry,
        bool expectedZero,
        bool expectedOverflow,
        bool expectedNegative)
    {
        // Arrange
        var emulator = new Emulator();
        emulator.Cpu.SetA(initialA);
        emulator.Cpu.SetX(xRegister);
        emulator.Cpu.SetY(0x24);
        emulator.Cpu.SetSp(0x00FD);
        emulator.Cpu.SetPc(0x0200);

        ushort effectiveAddr = (ushort)(baseAddress + xRegister);

        // Populate memory at effective target address and poison baseAddress if indexed
        emulator.Bus.Write(new Unassigned16Bits(effectiveAddr), new Unassigned8Bits(memoryValue));
        if (effectiveAddr != baseAddress)
        {
            emulator.Bus.Write(new Unassigned16Bits(baseAddress), new Unassigned8Bits(0xDD)); // Poison value
        }

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        emulator.Cpu.SetFlags(initialFlags);

        emulator.Bus.Write(new Unassigned16Bits(0x0200), new Unassigned8Bits(0x7D));
        emulator.Bus.Write(new Unassigned16Bits(0x0201), new Unassigned8Bits((byte)(baseAddress & 0xFF)));
        emulator.Bus.Write(new Unassigned16Bits(0x0202), new Unassigned8Bits((byte)(baseAddress >> 8)));

        // Act
        emulator.Step();

        // Assert 6502 Specification behavior
        emulator.Cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        emulator.Cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        emulator.Cpu.GetX().Should().Be(xRegister, "X register must remain unmodified");
        emulator.Cpu.GetY().Should().Be(0x24);
        emulator.Cpu.GetSp().Should().Be(0x00FD);
        emulator.Cpu.GetPc().Should().Be(0x0203);
        emulator.Bus.Read(new Unassigned16Bits(effectiveAddr)).Value.Should().Be(memoryValue, "Memory at effective target address must remain unmodified");
    }

    [Theory]
    // 6502 Specification Hardware-Correct ADC Absolute,Y (0x79) Scenarios:
    // Format: initialA, baseAddress, yRegister, memoryValue, initialCarry, expectedA, expectedCarry, expectedZero, expectedOverflow, expectedNegative
    [InlineData(0x10, (ushort)0x4000, 0x20, 0x20, false, 0x30, false, false, false, false)] // Simple addition without page cross
    [InlineData(0x00, (ushort)0x30FE, 0x03, 0x15, false, 0x15, false, false, false, false)] // Page boundary cross: 0x30FE + 0x03 = 0x3101 (crosses to page 0x31)
    [InlineData(0x00, (ushort)0xFFFF, 0x02, 0x42, false, 0x42, false, false, false, false)] // Full 16-bit address wrap: 0xFFFF + 0x02 = 0x0001
    [InlineData(0xFE, (ushort)0x1500, 0x05, 0x01, true,  0x00, true,  true,  false, false)] // Carry in + Carry out -> C set, Z set
    [InlineData(0xFF, (ushort)0x2500, 0x02, 0x01, false, 0x00, true,  true,  false, false)] // Carry out -> C set, Z set
    [InlineData(0x40, (ushort)0x3500, 0x05, 0x40, false, 0x80, false, false, true,  true)]  // Pos (64) + Pos (64) = Neg (-128) -> Signed Overflow V=T, N=T
    [InlineData(0x80, (ushort)0x45FF, 0x01, 0x80, false, 0x00, true,  true,  true,  false)] // Neg (-128) + Neg (-128) = Pos (0) -> Signed Overflow V=T, C=T, Z=T (page cross: 0x45FF+1=0x4600)
    [InlineData(0xD0, (ushort)0x5550, 0x10, 0x90, false, 0x60, true,  false, true,  false)] // Neg (-48) + Neg (-112) = Pos (+96) -> Signed Overflow V=T, C=T
    [InlineData(0x10, (ushort)0x6500, 0x01, 0x20, true,  0x31, false, false, false, false)] // Simple addition with Carry in
    public void ADC_AbsoluteYMode_ExecutesAdditionPerHardwareSpec(
        byte initialA,
        ushort baseAddress,
        byte yRegister,
        byte memoryValue,
        bool initialCarry,
        byte expectedA,
        bool expectedCarry,
        bool expectedZero,
        bool expectedOverflow,
        bool expectedNegative)
    {
        // Arrange
        var emulator = new Emulator();
        emulator.Cpu.SetA(initialA);
        emulator.Cpu.SetX(0x42);
        emulator.Cpu.SetY(yRegister);
        emulator.Cpu.SetSp(0x00FD);
        emulator.Cpu.SetPc(0x0200);

        ushort effectiveAddr = (ushort)(baseAddress + yRegister);

        // Populate memory at effective target address and poison baseAddress if indexed
        emulator.Bus.Write(new Unassigned16Bits(effectiveAddr), new Unassigned8Bits(memoryValue));
        if (effectiveAddr != baseAddress)
        {
            emulator.Bus.Write(new Unassigned16Bits(baseAddress), new Unassigned8Bits(0xDD)); // Poison value
        }

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        emulator.Cpu.SetFlags(initialFlags);

        emulator.Bus.Write(new Unassigned16Bits(0x0200), new Unassigned8Bits(0x79));
        emulator.Bus.Write(new Unassigned16Bits(0x0201), new Unassigned8Bits((byte)(baseAddress & 0xFF)));
        emulator.Bus.Write(new Unassigned16Bits(0x0202), new Unassigned8Bits((byte)(baseAddress >> 8)));

        // Act
        emulator.Step();

        // Assert 6502 Specification behavior
        emulator.Cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        emulator.Cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        emulator.Cpu.GetX().Should().Be(0x42);
        emulator.Cpu.GetY().Should().Be(yRegister, "Y register must remain unmodified");
        emulator.Cpu.GetSp().Should().Be(0x00FD);
        emulator.Cpu.GetPc().Should().Be(0x0203);
        emulator.Bus.Read(new Unassigned16Bits(effectiveAddr)).Value.Should().Be(memoryValue, "Memory at effective target address must remain unmodified");
    }

    [Theory]
    // 6502 Specification Hardware-Correct ADC Indexed Indirect (0x61) Scenarios:
    // Format: initialA, zpBase, xRegister, targetAddress, memoryValue, initialCarry, expectedA, expectedCarry, expectedZero, expectedOverflow, expectedNegative
    [InlineData(0x10, (byte)0x20, (byte)0x04, (ushort)0x1234, 0x20, false, 0x30, false, false, false, false)] // Simple Indexed Indirect addition
    [InlineData(0x00, (byte)0x80, (byte)0xFF, (ushort)0x2000, 0x15, false, 0x15, false, false, false, false)] // Zero page pointer wrap: 0x80 + 0xFF = 0x7F (in page 0)
    [InlineData(0x00, (byte)0xFE, (byte)0x01, (ushort)0x3000, 0x42, false, 0x42, false, false, false, false)] // Zero page pointer MSB wrap: ptr=0xFF, MSB at 0x0000
    [InlineData(0xFE, (byte)0x10, (byte)0x02, (ushort)0x0400, 0x01, true,  0x00, true,  true,  false, false)] // Carry in + Carry out -> C set, Z set
    [InlineData(0xFF, (byte)0x30, (byte)0x05, (ushort)0x0500, 0x01, false, 0x00, true,  true,  false, false)] // Carry out -> C set, Z set
    [InlineData(0x40, (byte)0x40, (byte)0x01, (ushort)0x4000, 0x40, false, 0x80, false, false, true,  true)]  // Pos (64) + Pos (64) = Neg (-128) -> Signed Overflow V=T, N=T
    [InlineData(0x80, (byte)0x50, (byte)0x02, (ushort)0x5000, 0x80, false, 0x00, true,  true,  true,  false)] // Neg (-128) + Neg (-128) = Pos (0) -> Signed Overflow V=T, C=T, Z=T
    [InlineData(0xD0, (byte)0x60, (byte)0x03, (ushort)0x6000, 0x90, false, 0x60, true,  false, true,  false)] // Neg (-48) + Neg (-112) = Pos (+96) -> Signed Overflow V=T, C=T
    [InlineData(0x10, (byte)0x70, (byte)0x01, (ushort)0x7000, 0x20, true,  0x31, false, false, false, false)] // Simple addition with Carry in
    public void ADC_IndexedIndirectMode_ExecutesAdditionPerHardwareSpec(
        byte initialA,
        byte zpBase,
        byte xRegister,
        ushort targetAddress,
        byte memoryValue,
        bool initialCarry,
        byte expectedA,
        bool expectedCarry,
        bool expectedZero,
        bool expectedOverflow,
        bool expectedNegative)
    {
        // Arrange
        var emulator = new Emulator();
        emulator.Cpu.SetA(initialA);
        emulator.Cpu.SetX(xRegister);
        emulator.Cpu.SetY(0x24);
        emulator.Cpu.SetSp(0x00FD);
        emulator.Cpu.SetPc(0x0200);

        // Calculate zero page pointer address wrapped to 8-bit range
        byte ptrAddr = (byte)(zpBase + xRegister);

        // Store 16-bit target address in Zero Page pointer (LSB at ptrAddr, MSB at (ptrAddr + 1) & 0xFF)
        emulator.Bus.Write(new Unassigned16Bits(ptrAddr), new Unassigned8Bits((byte)(targetAddress & 0xFF)));
        emulator.Bus.Write(new Unassigned16Bits((byte)(ptrAddr + 1)), new Unassigned8Bits((byte)((targetAddress >> 8) & 0xFF)));

        // Poison unindexed base pointer address if X > 0 and does not overwrite pointer LSB or MSB
        if (ptrAddr != zpBase && (byte)(ptrAddr + 1) != zpBase)
        {
            emulator.Bus.Write(new Unassigned16Bits(zpBase), new Unassigned8Bits(0xDD));
        }

        // Poison Page 1 if pointer MSB lookup crosses page boundary without wrapping
        ushort unwrappedPtrMsb = (ushort)(ptrAddr + 1);
        if (unwrappedPtrMsb > 0xFF)
        {
            emulator.Bus.Write(new Unassigned16Bits(unwrappedPtrMsb), new Unassigned8Bits(0xEE));
        }

        // Store memory value at target address
        emulator.Bus.Write(new Unassigned16Bits(targetAddress), new Unassigned8Bits(memoryValue));

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        emulator.Cpu.SetFlags(initialFlags);

        emulator.Bus.Write(new Unassigned16Bits(0x0200), new Unassigned8Bits(0x61));
        emulator.Bus.Write(new Unassigned16Bits(0x0201), new Unassigned8Bits(zpBase));

        // Act
        emulator.Step();

        // Assert 6502 Specification behavior
        emulator.Cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        emulator.Cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        emulator.Cpu.GetX().Should().Be(xRegister, "X register must remain unmodified");
        emulator.Cpu.GetY().Should().Be(0x24);
        emulator.Cpu.GetSp().Should().Be(0x00FD);
        emulator.Cpu.GetPc().Should().Be(0x0202);
        emulator.Bus.Read(new Unassigned16Bits(targetAddress)).Value.Should().Be(memoryValue, "Memory at target address must remain unmodified");
    }

    [Theory]
    // 6502 Specification Hardware-Correct ADC Indirect Indexed (0x71) Scenarios:
    // Format: initialA, zpAddr, yRegister, baseTargetAddr, memoryValue, initialCarry, expectedA, expectedCarry, expectedZero, expectedOverflow, expectedNegative
    [InlineData(0x10, (byte)0x20, (byte)0x10, (ushort)0x1200, 0x20, false, 0x30, false, false, false, false)] // Simple Indirect Indexed addition
    [InlineData(0x00, (byte)0x30, (byte)0x01, (ushort)0x20FF, 0x15, false, 0x15, false, false, false, false)] // Page boundary cross: 0x20FF + 0x01 = 0x2100
    [InlineData(0x00, (byte)0x40, (byte)0x02, (ushort)0xFFFF, 0x42, false, 0x42, false, false, false, false)] // Full 16-bit address wrap: 0xFFFF + 0x02 = 0x0001
    [InlineData(0x00, (byte)0xFF, (byte)0x05, (ushort)0x3000, 0x10, false, 0x10, false, false, false, false)] // Zero page pointer MSB wrap: zp=0xFF, MSB at 0x0000
    [InlineData(0xFE, (byte)0x10, (byte)0x02, (ushort)0x0400, 0x01, true,  0x00, true,  true,  false, false)] // Carry in + Carry out -> C set, Z set
    [InlineData(0xFF, (byte)0x50, (byte)0x05, (ushort)0x0500, 0x01, false, 0x00, true,  true,  false, false)] // Carry out -> C set, Z set
    [InlineData(0x40, (byte)0x60, (byte)0x01, (ushort)0x4000, 0x40, false, 0x80, false, false, true,  true)]  // Pos (64) + Pos (64) = Neg (-128) -> Signed Overflow V=T, N=T
    [InlineData(0x80, (byte)0x70, (byte)0x02, (ushort)0x5000, 0x80, false, 0x00, true,  true,  true,  false)] // Neg (-128) + Neg (-128) = Pos (0) -> Signed Overflow V=T, C=T, Z=T
    [InlineData(0xD0, (byte)0x80, (byte)0x03, (ushort)0x6000, 0x90, false, 0x60, true,  false, true,  false)] // Neg (-48) + Neg (-112) = Pos (+96) -> Signed Overflow V=T, C=T
    [InlineData(0x10, (byte)0x90, (byte)0x01, (ushort)0x7000, 0x20, true,  0x31, false, false, false, false)] // Simple addition with Carry in
    public void ADC_IndirectIndexedMode_ExecutesAdditionPerHardwareSpec(
        byte initialA,
        byte zpAddr,
        byte yRegister,
        ushort baseTargetAddr,
        byte memoryValue,
        bool initialCarry,
        byte expectedA,
        bool expectedCarry,
        bool expectedZero,
        bool expectedOverflow,
        bool expectedNegative)
    {
        // Arrange
        var emulator = new Emulator();
        emulator.Cpu.SetA(initialA);
        emulator.Cpu.SetX(0x42);
        emulator.Cpu.SetY(yRegister);
        emulator.Cpu.SetSp(0x00FD);
        emulator.Cpu.SetPc(0x0200);

        // Store base target address in Zero Page pointer (LSB at zpAddr, MSB at (zpAddr + 1) & 0xFF)
        emulator.Bus.Write(new Unassigned16Bits(zpAddr), new Unassigned8Bits((byte)(baseTargetAddr & 0xFF)));
        emulator.Bus.Write(new Unassigned16Bits((byte)(zpAddr + 1)), new Unassigned8Bits((byte)((baseTargetAddr >> 8) & 0xFF)));

        // Poison Page 1 if pointer MSB lookup crosses page boundary without wrapping
        ushort unwrappedPtrMsb = (ushort)(zpAddr + 1);
        if (unwrappedPtrMsb > 0xFF)
        {
            emulator.Bus.Write(new Unassigned16Bits(unwrappedPtrMsb), new Unassigned8Bits(0xEE));
        }

        // Calculate effective target address (16-bit addition with Y)
        ushort effectiveAddr = (ushort)(baseTargetAddr + yRegister);

        // Store memory value at effective target address
        emulator.Bus.Write(new Unassigned16Bits(effectiveAddr), new Unassigned8Bits(memoryValue));

        // Poison unindexed baseTargetAddr if Y > 0 and effectiveAddr != baseTargetAddr
        if (effectiveAddr != baseTargetAddr)
        {
            emulator.Bus.Write(new Unassigned16Bits(baseTargetAddr), new Unassigned8Bits(0xDD));
        }

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        emulator.Cpu.SetFlags(initialFlags);

        emulator.Bus.Write(new Unassigned16Bits(0x0200), new Unassigned8Bits(0x71));
        emulator.Bus.Write(new Unassigned16Bits(0x0201), new Unassigned8Bits(zpAddr));

        // Act
        emulator.Step();

        // Assert 6502 Specification behavior
        emulator.Cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        emulator.Cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        emulator.Cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        emulator.Cpu.GetX().Should().Be(0x42);
        emulator.Cpu.GetY().Should().Be(yRegister, "Y register must remain unmodified");
        emulator.Cpu.GetSp().Should().Be(0x00FD);
        emulator.Cpu.GetPc().Should().Be(0x0202);
        emulator.Bus.Read(new Unassigned16Bits(effectiveAddr)).Value.Should().Be(memoryValue, "Memory at effective target address must remain unmodified");
    }
}
