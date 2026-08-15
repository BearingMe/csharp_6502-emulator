# AGENTS.md

## Project Overview
MOS 6502 microprocessor emulator written in C# targeting .NET 9.

## Tech Stack
- **Language**: C# 13 / .NET 9 (`net9.0`)
- **Build Tool / SDK**: .NET SDK (`dotnet`)
- **Testing**: xUnit

## Commands

```bash
# Build
dotnet build

# Test
dotnet test

# Clean
dotnet clean
```

## Overview
C# 6502 emulator study project. Knowledge assistance & test writing only.

## Context & Detailed Guidelines
- Architecture & Module Boundaries: See `docs/architecture.md`
- C# 13 Coding Style & Conventions: See `docs/styleguides/csharp.md`
- Testing Strategy & Conventions: See `docs/testing.md`
- 6502 Hardware Specifications: See `docs/hardware-specification/index.md`

## Key Rules
- **Fixed-width integer usage**: Always use explicit integer types (`byte`, `ushort`) for 6502 registers and 16-bit address memory pointers.
- **Architectural boundaries**: Business/emulator domain logic must remain pure and free from console/CLI dependencies.
- **No external dependencies**: Core emulator logic must remain standard library pure .NET 9.

## Unit Tests
- Every instruction has exactly 1 unit test function for each addressing mode using xUnit [Theory] with [InlineData] for branches.
- Verify expected target changes and assert non-targeted state remains unmodified.

## Regression Tests
- Follow the local skill rules in .opencode/skills/rstest-regression-test/SKILL.md.
- Must assert spec-correct behavior (never buggy behavior).
- Must FAIL against current bug and PASS when fixed.

## Never
- Never edit src/.
- Never write regression tests that expect buggy behavior or pass while bug exists.
- Never weaken assertions to force a test pass.
- Never run GUI/visual execution commands.
- Never commit build output (`bin/`, `obj/`).

## Definition of Done
Work is complete ONLY when:
1. All code follows architectural boundaries in `docs/architecture.md` and style conventions in `docs/styleguides/csharp.md`.
2. `dotnet build` succeeds with zero errors/warnings.
3. `dotnet test` succeeds and all unit/integration tests pass.
4. @adversary review passes.

