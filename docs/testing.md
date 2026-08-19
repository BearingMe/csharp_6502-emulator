# Testing Strategy

## Stack & Frameworks

- **Runner & Framework**: xUnit (`dotnet test`)
- **Assertion Style**: FluentAssertions (`.Should().Be()`)
- **Mocking Policy**: No mocking framework used. Core CPU & memory logic must use real, lightweight domain instances (e.g., standard byte arrays/memory buffers).
- **Target Platform**: .NET 9 (`net9.0`)

> *(For execution commands, see `AGENTS.md`.)*

---

## Test Pyramid & Structure

- **Domain Unit Tests (`tests/Domain.Tests/`)**: Verify individual 6502 instructions, addressing mode resolution, memory operations, and CPU register/flag mutations.
- **Integration / Functional Tests (`tests/Functional/`)**: Execute compiled 6502 binary programs (e.g., Klaus Dormann 6502 functional test suite) to verify end-to-end CPU execution correctness.
- **E2E / CLI Tests (`tests/E2E/`)**: Verify CLI execution, binary file/ROM loading, and application entry points.

---

## Unit Testing Conventions

### Test Methods & Structure
- **File Naming & Location**: Unit test files reside under `tests/Domain.Tests/` (e.g., `AddresingModeTests.cs`, `InstructionTests.cs`, `MemoryTests.cs`, `RegistersTests.cs`).
- **Naming Pattern**: `[MethodOrOpcode]_[Scenario]_[ExpectedResult]`
  - Example: `ADC_ProducesExpectedResultAndFlags` or `Immediate_ReturnsOperand`
- **Triple-A Pattern**: Explicitly demarcate `// Arrange`, `// Act`, `// Assert` blocks when applicable.
- **Single Behavior per Test**: Assert targeted register/flag changes while verifying non-targeted state remains strictly unmodified.

### Data-Driven Testing
- **Addressing Modes & Branch Scenarios**: Every instruction must have unit test coverage for each addressing mode, utilizing `[Theory]` with `[InlineData]` for parameterizing values/branches.

```csharp
[Theory]
[InlineData(0x10, 0x20, 0x30, false, false, false, false)]
[InlineData(0xFF, 0x01, 0x00, true, true, false, false)]
public void ADC_ProducesExpectedResultAndFlags(
    byte accumulator,
    byte operand,
    byte expectedResult,
    bool expectedCarry,
    bool expectedZero,
    bool expectedOverflow,
    bool expectedNegative)
{
    // Arrange
    cpu.Accumulator = accumulator;
    cpu.Flags = Status.Interrupt;

    // Act
    cpu.ADC(AddressingMode.Immediate, operand);

    // Assert
    Assert.Equal(expectedResult, cpu.Accumulator.Value);
    Assert.Equal(expectedCarry, cpu.Flags.HasFlag(Status.Carry));
    Assert.Equal(expectedZero, cpu.Flags.HasFlag(Status.Zero));
    Assert.Equal(expectedOverflow, cpu.Flags.HasFlag(Status.Overflow));
    Assert.Equal(expectedNegative, cpu.Flags.HasFlag(Status.Negative));
}
```

### Determinism & Setup
- **State Initialization**: Every CPU unit test must initialize CPU registers (`A`, `X`, `Y`, `PC`, `SP`) and memory to a known state before executing an instruction.
- **Helper Methods**: Use private factory helper methods (e.g., `CreateCpu()`) for common setup instead of constructor setup.

---

## What NOT to Test

- Do not test built-in C# runtime functions or simple getter/setter auto-properties.
- Do not test framework/library internals.
- Do not mock internal CPU state, registers, or memory buffers — test opcode execution through real state mutations.
- Do not test console output or CLI dependencies within unit test files (keep emulator domain tests standard-library pure).

---

## Test Data & Fixtures

- **Hex Literals**: Opcode verification and address calculations must use explicit hex literals (`0xA9`, `0xFF00`, `0x00FD`).
- **No Magic Numbers**: Define constants for key addresses or expected masks if reused across multiple assertions in a test.
- **ROM Fixtures**: Binary test fixtures for functional suites reside in `tests/Fixtures/Roms/`.

---

## Flakiness & Determinism Policy

- CPU emulator tests are strictly deterministic. Randomness, system clock dependencies, and async non-determinism are prohibited in unit test suites.
- If a test fails intermittently, it represents an uninitialized state bug or side-effect leakage, not a network/flaky environment issue.

---

## Code Coverage Strategy

- **Core Opcode Coverage**: 100% coverage expected for opcode dispatchers and instruction cycle updates.
- **Coverage Execution**: Tracked via `dotnet test --collect:"XPlat Code Coverage"`.
