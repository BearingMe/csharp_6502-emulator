# C# 14 / .NET 10 Style Guide

Official Reference: [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)

## Naming Conventions

- **PascalCase**: Classes, structs, enums, interfaces (prefixed with `I`), methods, properties, and public fields.
  - Example: `CpuRegisters`, `IMemoryBus`, `StepInstruction()`

- **camelCase**: Private fields (prefixed with `_`), parameters, and local variables.
  - Example: `_programCounter`, `addressingMode`, `byteValue`

- **Constants**: PascalCase.
  - Example: `ZeroFlagMask`

- **Enum values**: PascalCase.
  - Example: `Status.Zero`

- **File Names**: Match the primary type name in PascalCase.
  - Example: `CpuState.cs`

## Syntax Choices & C# 14 / .NET 10 Idioms

- **Implicit Usings & Nullable Reference Types**: Enabled. Treat warnings as errors where practical.

- **Primary Constructors**: Use when they make dependencies and initialization clearer. Do not use them merely because they are available.
  - Example: `public class MemoryBus(byte[] ram) { ... }`

- **Pattern Matching**: Prefer `switch` expressions and pattern matching when they make branching logic clearer, especially for opcode decoding and bit manipulation.

- **Fixed-width Types**: Use `byte` and `ushort` when they represent actual 6502 values, registers, addresses, or memory values. Use `int` when it is the natural type for C# operations such as collection indexes or counters.

- **Expression-bodied Members**: Use for genuinely simple single-expression members. Prefer normal blocks when they improve readability.

## Code Quality & Architecture

- **Prefer simplicity over abstraction**: Do not introduce interfaces, factories, wrappers, base classes, dependency injection, or helper abstractions without a concrete need.

- **Minimize dependencies**: A class should receive only the dependencies it actually needs. Avoid passing large object graphs through layers merely to access one value.

- **Keep domain logic direct**: CPU instructions, addressing modes, registers, flags, and memory operations should be easy to follow from the code itself.

- **Prefer composition over indirection**: Small, independent components are preferred over deep abstraction hierarchies.

- **Avoid premature organization**: Do not create files, classes, or namespaces solely because a file became large. Organize around meaningful domain concepts.

- **Refactor from repetition**: Introduce an abstraction when repeated code demonstrates a real shared concept, not because similar code looks aesthetically uncomfortable.

- **Optimize for readability**: A few repeated lines are preferable to a helper that forces the reader to navigate elsewhere to understand simple logic.

- **Do not optimize for line count**: Shorter code is not automatically better code. Optimize for the amount of information a reader must hold in their head.

- **Minimize indirection**: Prefer code that can be understood locally without navigating through multiple layers of helpers or abstractions.

- **Avoid speculative flexibility**: Do not design APIs for hypothetical future requirements.

- **Keep dependencies flowing inward**: Low-level domain concepts should not depend on high-level orchestration or unrelated components.

## Tests

- Tests are executable specifications and must be readable as documentation.

- **Test behavior, not implementation**: Tests should verify the 6502 specification and observable behavior, not internal algorithms, method calls, or class structure.

- **Keep tests minimal**: A test should contain only the setup, operation, and assertions necessary to express its behavior.

- **Keep setup local**: Prefer explicit setup inside the test over helpers that hide important information.

- **Prefer direct tests**: A few repeated lines are better than test helpers, builders, fixtures, or abstractions that make the scenario harder to understand.

- **Use `[Fact]` by default**. Use `[Theory]` when multiple inputs represent the same behavior.

- **Keep related tests together**: Do not create many small files merely to reduce file size.

- **Do not optimize for test file size**: A large cohesive test file is preferable to many fragmented files when the tests describe the same concept.

- **Use real lightweight domain objects**: Do not mock CPU state, registers, memory, or other core emulator components.

- **Avoid testing implementation details**: Do not assert private state, internal call sequences, or incidental implementation choices unless they are observable requirements of the 6502 specification.

## Anti-Patterns

- **Do not use magic numbers for bitwise operations**: Define bitmasks as `const byte` values or enum flags.
  - Example: `Status.Zero` or `ZeroFlagMask`

- **Do not hide implicit numeric conversions**: Be explicit when converting between `byte`, `ushort`, `int`, and other numeric types, especially when implementing 6502 wrapping arithmetic.

- **Do not use generic `Exception`**: Throw a specific exception such as `InvalidOperationException` or a domain-specific exception.

- **Do not create abstractions to eliminate trivial repetition**.

- **Do not create helpers solely to shorten a method or test**.

- **Do not mirror the source structure solely for organizational symmetry**.

- **Do not introduce dependency injection or interfaces where direct construction is simpler**.

- **Do not add code for hypothetical requirements**.

- **Do not sacrifice readability for cleverness or excessive use of C# features.**