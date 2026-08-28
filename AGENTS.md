# AGENTS.md

## Project

MOS 6502 microprocessor emulator written in C# 14 targeting .NET 10.

## Commands

```bash
dotnet build
dotnet test
dotnet run --project src/mos6502.csproj
dotnet clean
```

## References

Follow these documents when applicable:

- `docs/architecture.md` — Architecture and domain boundaries
- `docs/styleguides/csharp.md` — C# coding standards and code quality
- `docs/testing.md` — Testing strategy and conventions
- `docs/hardware-specification/index.md` — 6502 hardware specification

When rules conflict, the hardware specification takes precedence for emulator behavior.

## Core Rules

### Specification Is the Source of Truth

Tests must verify the actual 6502 specification.

Never change a test to accommodate an incorrect implementation.

If the implementation disagrees with the specification, the test must represent the specification.

### Simplicity

Prefer the smallest solution that clearly expresses the required behavior.

Do not introduce unnecessary:

- Abstractions
- Helpers
- Fixtures
- Interfaces
- Factories
- IoC containers or dependency injection frameworks
- Indirection

Prefer direct constructor dependencies and readable repetition over abstractions that hide important behavior.

### Architecture

- Keep core emulator logic independent of CLI and application concerns.
- Keep core emulator logic dependent only on the .NET standard library.
- Follow the boundaries defined in `docs/architecture.md`.
- Keep the folder structure: `src/Domain`, `src/Application`, and `src/Infrastructure`.
- Tests mirror `src/`.

### Types

Use fixed-width types where they represent actual 6502 values:

- `byte` for 8-bit values
- `ushort` for 16-bit addresses

Use explicit conversions when crossing numeric widths or implementing wrapping arithmetic.

## Tests

Tests must be:

- Minimal
- Human-readable
- Deterministic
- Independent
- Specification-driven
- Focused on observable behavior

Test the behavior of the 6502, not implementation details.

Keep setup local and explicit.

Use real lightweight domain objects. Do not mock CPU state, registers, memory, or other core emulator components.

Use `[Fact]` by default.

Use `[Theory]` when multiple inputs exercise the same behavior. Prefer `[InlineData]` for small datasets.

Do not force unrelated scenarios into one theory.

A test may contain multiple assertions when they collectively verify one specified behavior.

Do not require one assertion per test.

Tests should verify all relevant observable state affected by the behavior being tested.

## Never

- Never write tests that expect incorrect 6502 behavior.
- Never weaken assertions to make tests pass.
- Never modify the specification to match the implementation.
- Never introduce test abstractions solely to reduce line count.
- Never test private implementation details.
- Never run GUI or visual execution commands.
- Never commit `bin/` or `obj/`.

## Definition of Done

Work is complete only when:

1. Tests follow `docs/testing.md`.
2. Code quality and test code follow `docs/styleguides/csharp.md`.
3. Architecture remains consistent with `docs/architecture.md`.
4. Tests assert behavior defined by the 6502 specification.
5. `dotnet build` succeeds with zero errors and warnings.
6. `dotnet test` succeeds.
7. Regression tests fail before the corresponding bug fix and pass afterward.
8. `@adversary` review passes.