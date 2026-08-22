# Testing Strategy

## Stack

- **Framework**: xUnit
- **Assertions**: FluentAssertions
- **Mocking**: No mocking framework. Use real CPU, bus, memory, and other lightweight domain objects.
- **Target**: .NET 10 (`net10.0`)

See `AGENTS.md` for test commands.

---

## Testing Principles

Tests are executable specifications.

They must be:

- **Minimal**: contain only what is necessary to express the behavior.
- **Human-readable**: a reader should understand the scenario and expected result immediately.
- **Specification-driven**: test what the 6502 is required to do, not how the emulator happens to implement it.
- **Deterministic**: the same input must always produce the same result.
- **Independent**: tests must not depend on execution order or shared mutable state.
- **Direct**: avoid unnecessary helpers, fixtures, abstractions, and indirection.

Prefer a few repeated lines over a helper that hides important behavior.

Do not organize tests around implementation details merely to mirror the source code.

---

## Test Levels

### Unit Tests

Test CPU behavior in isolation using real domain objects.

Cover:

- Instruction behavior
- Addressing modes
- Registers
- Status flags
- Memory behavior
- Program counter and stack behavior
- Instruction timing when observable through the specification

Tests should exercise behavior through the public domain API rather than internal implementation details.

### Functional Tests

Execute complete 6502 programs against the emulator.

Use these to verify that independently correct instructions, addressing modes, memory, flags, and control flow work together correctly.

Examples include the Klaus Dormann 6502 functional test suite.

### E2E Tests

Verify application-level behavior such as:

- CLI execution
- Binary loading
- ROM loading
- Application entry points

Keep these separate from domain tests.

---

## Unit Test Conventions

### Naming

Names should describe the behavior being specified:

`[Operation]_[Scenario]_[ExpectedBehavior]`

Examples:

```csharp
Lda_Immediate_LoadsAccumulator
Adc_WithCarry_SetsCarryFlag
Jmp_Absolute_SetsProgramCounter
```

Avoid names such as:

```csharp
TestLda()
ExecuteInstruction()
TestCase1()
```

A failed test name should explain what behavior is broken.

### Test Structure

Keep the test itself obvious.

```csharp
[Fact]
public void Lda_Immediate_LoadsAccumulator()
{
    cpu.Load(0xA9, 0x42);

    cpu.Step();

    cpu.A.Should().Be(0x42);
}
```

Do not require explicit `// Arrange`, `// Act`, and `// Assert` comments. The code should make those phases obvious.

Do not extract setup into helpers unless the helper removes substantial noise without hiding the scenario.

### Assertions

Assert the observable behavior required by the specification.

An instruction may legitimately require several assertions:

```csharp
[Fact]
public void Adc_SetsResultAndFlags()
{
    cpu.A = 0xFF;
    cpu.Load(0x69, 0x01);

    cpu.Step();

    cpu.A.Should().Be(0x00);
    cpu.Status.Should().HaveFlag(Status.Carry);
    cpu.Status.Should().HaveFlag(Status.Zero);
}
```

Do not assert internal implementation details, private state, call sequences, or incidental memory accesses unless the 6502 specification explicitly makes them observable.

### State

Initialize only the state relevant to the scenario.

Do not reset every register and every byte of memory in every test if the behavior being tested does not depend on them.

When unspecified state matters to correctness, initialize it explicitly so the test documents the assumption.

---

## Data-Driven Tests

Use `[Theory]` when several inputs exercise the **same specified behavior**.

Prefer `[InlineData]` for small datasets:

```csharp
[Theory]
[InlineData(0x00, true)]
[InlineData(0x01, false)]
[InlineData(0x80, false)]
public void Lda_SetsZeroFlagCorrectly(byte value, bool expected)
{
    cpu.Load(0xA9, value);

    cpu.Step();

    cpu.Status.HasFlag(Status.Zero).Should().Be(expected);
}
```

Do not force unrelated scenarios into one theory merely to reduce lines.

Prefer separate `[Fact]` tests when each case describes a different rule.

Use `MemberData` or `ClassData` only when the data cannot remain clear with `InlineData`.

---

## 6502 Specification Coverage

Instructions should be tested against the behavior defined by the 6502 specification.

Where an instruction supports multiple addressing modes, cover the relevant addressing modes.

Tests should verify:

- Resulting values
- Required status flags
- Program counter changes
- Stack effects
- Memory effects
- Page crossing behavior where applicable
- Branch behavior
- Cycle counts when exposed by the emulator's contract
- Read-modify-write behavior where required

Do not create tests for every possible combination blindly.

Choose cases that demonstrate the actual rules and edge cases of the specification.

For example, arithmetic instructions should include cases that exercise carry, zero, overflow, and negative behavior rather than merely repeating ordinary values.

---

## Test Data

Use explicit hexadecimal values for 6502 values when they improve readability:

```csharp
0xA9
0xFF
0x8000
0x00FD
```

Do not introduce constants for a value used once simply to give it a name.

Use constants when a value is reused or its meaning would otherwise be unclear.

Test data should make the 6502 behavior recognizable without requiring conversion from decimal.

---

## What Not to Test

Do not test:

- C# runtime behavior
- Framework behavior
- Library internals
- Trivial auto-properties
- Private implementation details
- Internal method call structure
- Mock interactions
- Incidental implementation choices

Do not mock CPU state, registers, memory, or other core emulator components.

Use real lightweight domain objects.

---

## Fixtures and Helpers

Avoid fixtures and helpers for ordinary CPU setup.

Prefer:

```csharp
var cpu = new Cpu(memory);
cpu.A = 0x10;
cpu.Load(0x69, 0x20);
```

over:

```csharp
var cpu = CreateInitializedCpuWithProgram(0x69, 0x20);
```

The latter hides information that is important to understanding the test.

Use fixtures only for genuinely expensive shared resources, such as functional-test environments or external infrastructure.

---

## Functional Test Fixtures

Binary ROM and program fixtures belong in the functional test environment.

Keep reusable ROMs under:

`tests/Fixtures/Roms/`

Functional tests should verify complete programs rather than duplicating every instruction-level assertion already covered by unit tests.

---

## Determinism

CPU tests must be deterministic.

Do not use:

- Random values without a fixed seed
- System time
- External services
- Network access
- Uncontrolled concurrency
- Environment-dependent behavior

An intermittent CPU test failure should normally indicate a bug in the emulator or test setup, not an unreliable test environment.

---

## Coverage

Coverage is a signal, not the goal.

The important requirement is that the 6502 specification is meaningfully covered, including edge cases and instruction behavior.

Do not add tests solely to increase a coverage percentage.

Run coverage with:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

100% coverage of a method does not guarantee correct emulation. A test can execute every branch while completely missing an important 6502 rule.

Correctness against the specification takes priority over coverage numbers.