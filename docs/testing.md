# Testing Strategy

## Stack & Frameworks

- **Runner & Framework**: xUnit (`dotnet test`)
- **Assertion Style**: FluentAssertions (`.Should().Be()`)
- **Mocking Policy**: No mocking framework used. Core CPU & memory logic must use real, lightweight domain instances (e.g., standard byte arrays/memory buffers).
- **Target Platform**: .NET 9 (`net9.0`)

> *(For execution commands, see `AGENTS.md`.)*

---

## Test Pyramid & Structure

- **Unit Tests (`tests/Unit/`)**: Verify individual 6502 instructions/opcodes, addressing mode calculations, cycle counts, and register/flag mutations.
- **Integration / Functional Tests (`tests/Functional/`)**: Execute compiled 6502 binary programs (e.g., Klaus Dormann 6502 functional test suite) to verify end-to-end CPU execution correctness.
- **E2E / CLI Tests (`tests/E2E/`)**: Verify CLI execution, binary file/ROM loading, and application entry points.

---

## Unit Testing Conventions

### Test Methods & Structure
- **File Naming & Location**: Unit test files mirror domain classes under `tests/` (e.g., `tests/Cpu/AdcOpcodeTests.cs`).
- **Naming Pattern**: `[MethodOrOpcode]_[Scenario]_[ExpectedResult]`
  - Example: `ADC_ImmediateMode_SetsZeroFlagWhenResultIsZero`
- **Triple-A Pattern**: Explicitly demarcate `// Arrange`, `// Act`, `// Assert` blocks.
- **Single Behavior per Test**: Assert targeted register/flag changes while verifying non-targeted state remains strictly unmodified.

### Data-Driven Testing
- **Addressing Modes & Branch Scenarios**: Every instruction must have exactly 1 unit test function for each addressing mode, utilizing `[Theory]` with `[InlineData]` for parameterizing values/branches.

```csharp
[Theory]
[InlineData(0x00, true)]
[InlineData(0x01, false)]
public void LDA_Immediate_UpdatesZeroFlag(byte value, bool expectedZero)
{
    // Arrange
    var cpu = CreateCpu();

    // Act
    cpu.ExecuteLdaImmediate(value);

    // Assert
    cpu.Flags.HasFlag(Status.Zero).Should().Be(expectedZero);
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
