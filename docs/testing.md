# Testing Strategy

## Stack & Frameworks

- **Runner & Framework**: xUnit (`dotnet test`)
- **Assertion Style**: FluentAssertions or standard xUnit assertions (`Assert.Equal`, `Assert.True`).
- **Mocking**: Moq or NSubstitute (if needed for interface boundaries like `IMemoryBus`).
- **Target Platform**: .NET 9 (`net9.0`).

(For execution commands, see `AGENTS.md`.)

## Test Pyramid & Structure

- **Unit Tests (`tests/Unit/`)**: Focus on individual 6502 opcode execution, instruction timing/cycles, addressing mode address calculation, and status flag updates.
- **Integration / Functional Tests (`tests/Functional/`)**: Execute compiled 6502 binary programs (e.g., standard Klaus Dormann 6502 functional test suite) to verify end-to-end CPU correctness.
- **E2E / CLI Tests (`tests/E2E/`)**: Command-line interface execution and ROM loading tests.

## Unit Testing Conventions

- **File Naming & Location**: Unit test files mirror domain classes under `tests/` (e.g., `tests/Cpu/AdcOpcodeTests.cs`).
- **Naming Pattern**: `[MethodOrOpcode]_[Scenario]_[ExpectedResult]`
  - Example: `ADC_ImmediateMode_SetsZeroFlagWhenResultIsZero()`
- **Triple-A Pattern**: Every test must clearly demarcate `// Arrange`, `// Act`, `// Assert`.
- **Determinism**: Every CPU test must initialize CPU registers and memory to a known state before executing an instruction.

## What NOT to Test

- Do not write unit tests for standard C# built-in library functions or basic getter/setter auto-properties.
- Do not mock internal CPU registers or internal bitwise helpers when testing opcode execution — test opcodes via real CPU state mutations.

## Test Data

- Opcode verification tests should use explicit byte values and hex literals (`0xA9`, `0x00`, `0xFF`).
- Functional test suite ROM files sit in `tests/Fixtures/Roms/`.
