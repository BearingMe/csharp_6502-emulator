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
        var cpu = new mos6502.src.Cpu();
        cpu.SetA(initialA);
        cpu.SetX(0x42);
        cpu.SetY(0x24);
        cpu.SetSp(0x00FD);
        cpu.SetPc(0x0200);

        // Populate Zero Page memory
        cpu.GetMemory()[zeroPageAddr] = memoryValue;

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        cpu.SetFlags(initialFlags);

        var instruction = new Instruction(0x65, zeroPageAddr);

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
        cpu.GetMemory()[zeroPageAddr].Should().Be(memoryValue, "Memory at zero page address must remain unmodified");
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
        var cpu = new mos6502.src.Cpu();
        cpu.SetA(initialA);
        cpu.SetX(xRegister);
        cpu.SetY(0x24);
        cpu.SetSp(0x00FD);
        cpu.SetPc(0x0200);

        // Effective zero-page address wrapped to 8-bit range (0x00-0xFF) per 6502 spec
        byte effectiveAddr = (byte)(zeroPageAddr + xRegister);

        // Populate target Zero Page memory and set poison value at unwrapped Page 1 address and base address to verify indexing & wrapping
        cpu.GetMemory()[effectiveAddr] = memoryValue;
        if (effectiveAddr != zeroPageAddr)
        {
            cpu.GetMemory()[zeroPageAddr] = 0xDD; // Poison value if emulator incorrectly reads unindexed base address
        }
        ushort unwrappedAddr = (ushort)(zeroPageAddr + xRegister);
        if (unwrappedAddr > 0xFF)
        {
            cpu.GetMemory()[unwrappedAddr] = 0xEE; // Poison value if emulator incorrectly accesses Page 1
        }

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        cpu.SetFlags(initialFlags);

        var instruction = new Instruction(0x75, zeroPageAddr);

        // Act
        cpu.Execute(instruction);

        // Assert 6502 Specification behavior
        cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        cpu.GetX().Should().Be(xRegister, "X register must remain unmodified");
        cpu.GetY().Should().Be(0x24);
        cpu.GetSp().Should().Be(0x00FD);
        cpu.GetPc().Should().Be(0x0200);
        cpu.GetMemory()[effectiveAddr].Should().Be(memoryValue, "Memory at effective zero-page address must remain unmodified");
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
        var cpu = new mos6502.src.Cpu();
        cpu.SetA(initialA);
        cpu.SetX(0x42);
        cpu.SetY(0x24);
        cpu.SetSp(0x00FD);
        cpu.SetPc(0x0200);

        // Populate memory at 16-bit target address
        cpu.GetMemory()[address] = memoryValue;

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        cpu.SetFlags(initialFlags);

        var instruction = new Instruction(0x6D, address);

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
        cpu.GetMemory()[address].Should().Be(memoryValue, "Memory at target address must remain unmodified");
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
        var cpu = new mos6502.src.Cpu();
        cpu.SetA(initialA);
        cpu.SetX(xRegister);
        cpu.SetY(0x24);
        cpu.SetSp(0x00FD);
        cpu.SetPc(0x0200);

        ushort effectiveAddr = (ushort)(baseAddress + xRegister);

        // Populate memory at effective target address and poison baseAddress if indexed
        cpu.GetMemory()[effectiveAddr] = memoryValue;
        if (effectiveAddr != baseAddress)
        {
            cpu.GetMemory()[baseAddress] = 0xDD; // Poison value if emulator reads unindexed base address
        }

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        cpu.SetFlags(initialFlags);

        var instruction = new Instruction(0x7D, baseAddress);

        // Act
        cpu.Execute(instruction);

        // Assert 6502 Specification behavior
        cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        cpu.GetX().Should().Be(xRegister, "X register must remain unmodified");
        cpu.GetY().Should().Be(0x24);
        cpu.GetSp().Should().Be(0x00FD);
        cpu.GetPc().Should().Be(0x0200);
        cpu.GetMemory()[effectiveAddr].Should().Be(memoryValue, "Memory at effective target address must remain unmodified");
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
        var cpu = new mos6502.src.Cpu();
        cpu.SetA(initialA);
        cpu.SetX(0x42);
        cpu.SetY(yRegister);
        cpu.SetSp(0x00FD);
        cpu.SetPc(0x0200);

        ushort effectiveAddr = (ushort)(baseAddress + yRegister);

        // Populate memory at effective target address and poison baseAddress if indexed
        cpu.GetMemory()[effectiveAddr] = memoryValue;
        if (effectiveAddr != baseAddress)
        {
            cpu.GetMemory()[baseAddress] = 0xDD; // Poison value if emulator reads unindexed base address
        }

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        cpu.SetFlags(initialFlags);

        var instruction = new Instruction(0x79, baseAddress);

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
        cpu.GetY().Should().Be(yRegister, "Y register must remain unmodified");
        cpu.GetSp().Should().Be(0x00FD);
        cpu.GetPc().Should().Be(0x0200);
        cpu.GetMemory()[effectiveAddr].Should().Be(memoryValue, "Memory at effective target address must remain unmodified");
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
        var cpu = new mos6502.src.Cpu();
        cpu.SetA(initialA);
        cpu.SetX(xRegister);
        cpu.SetY(0x24);
        cpu.SetSp(0x00FD);
        cpu.SetPc(0x0200);

        // Calculate zero page pointer address wrapped to 8-bit range
        byte ptrAddr = (byte)(zpBase + xRegister);

        // Store 16-bit target address in Zero Page pointer (LSB at ptrAddr, MSB at (ptrAddr + 1) & 0xFF)
        cpu.GetMemory()[ptrAddr] = (byte)(targetAddress & 0xFF);
        cpu.GetMemory()[(byte)(ptrAddr + 1)] = (byte)((targetAddress >> 8) & 0xFF);

        // Poison unindexed base pointer address if X > 0 and does not overwrite pointer LSB or MSB
        if (ptrAddr != zpBase && (byte)(ptrAddr + 1) != zpBase)
        {
            cpu.GetMemory()[zpBase] = 0xDD;
        }

        // Poison Page 1 if pointer MSB lookup crosses page boundary without wrapping
        ushort unwrappedPtrMsb = (ushort)(ptrAddr + 1);
        if (unwrappedPtrMsb > 0xFF)
        {
            cpu.GetMemory()[unwrappedPtrMsb] = 0xEE;
        }

        // Store memory value at target address
        cpu.GetMemory()[targetAddress] = memoryValue;

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        cpu.SetFlags(initialFlags);

        var instruction = new Instruction(0x61, zpBase);

        // Act
        cpu.Execute(instruction);

        // Assert 6502 Specification behavior
        cpu.GetA().Should().Be(expectedA, "Accumulator must match spec result");
        cpu.GetFlags().HasFlag(Status.Carry).Should().Be(expectedCarry, "Carry flag must match spec");
        cpu.GetFlags().HasFlag(Status.Zero).Should().Be(expectedZero, "Zero flag must match spec");
        cpu.GetFlags().HasFlag(Status.Overflow).Should().Be(expectedOverflow, "Overflow flag V must reflect signed overflow per 6502 spec");
        cpu.GetFlags().HasFlag(Status.Negative).Should().Be(expectedNegative, "Negative flag N must reflect bit 7 of result");

        // Assert non-targeted state remains unmodified
        cpu.GetX().Should().Be(xRegister, "X register must remain unmodified");
        cpu.GetY().Should().Be(0x24);
        cpu.GetSp().Should().Be(0x00FD);
        cpu.GetPc().Should().Be(0x0200);
        cpu.GetMemory()[targetAddress].Should().Be(memoryValue, "Memory at target address must remain unmodified");
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
        var cpu = new mos6502.src.Cpu();
        cpu.SetA(initialA);
        cpu.SetX(0x42);
        cpu.SetY(yRegister);
        cpu.SetSp(0x00FD);
        cpu.SetPc(0x0200);

        // Store base target address in Zero Page pointer (LSB at zpAddr, MSB at (zpAddr + 1) & 0xFF)
        cpu.GetMemory()[zpAddr] = (byte)(baseTargetAddr & 0xFF);
        cpu.GetMemory()[(byte)(zpAddr + 1)] = (byte)((baseTargetAddr >> 8) & 0xFF);

        // Poison Page 1 if pointer MSB lookup crosses page boundary without wrapping
        ushort unwrappedPtrMsb = (ushort)(zpAddr + 1);
        if (unwrappedPtrMsb > 0xFF)
        {
            cpu.GetMemory()[unwrappedPtrMsb] = 0xEE;
        }

        // Calculate effective target address (16-bit addition with Y)
        ushort effectiveAddr = (ushort)(baseTargetAddr + yRegister);

        // Store memory value at effective target address
        cpu.GetMemory()[effectiveAddr] = memoryValue;

        // Poison unindexed baseTargetAddr if Y > 0 and effectiveAddr != baseTargetAddr
        if (effectiveAddr != baseTargetAddr)
        {
            cpu.GetMemory()[baseTargetAddr] = 0xDD;
        }

        Status initialFlags = Status.Interrupt;
        if (initialCarry)
        {
            initialFlags |= Status.Carry;
        }
        cpu.SetFlags(initialFlags);

        var instruction = new Instruction(0x71, zpAddr);

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
        cpu.GetY().Should().Be(yRegister, "Y register must remain unmodified");
        cpu.GetSp().Should().Be(0x00FD);
        cpu.GetPc().Should().Be(0x0200);
        cpu.GetMemory()[effectiveAddr].Should().Be(memoryValue, "Memory at effective target address must remain unmodified");
    }
}
