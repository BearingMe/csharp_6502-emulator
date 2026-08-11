# C# 13 / .NET 9 Style Guide

Official Reference: [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)

## Naming Conventions

- **PascalCase**: Classes, Structs, Enums, Interfaces (prefixed with `I`), Methods, Properties, Public fields.
  - Example: `CpuRegisters`, `IMemoryBus`, `StepInstruction()`
- **camelCase**: Private instance fields (prefixed with `_`), Method parameters, Local variables.
  - Example: `_programCounter`, `addressingMode`, `byteValue`
- **ALL_CAPS / PascalCase**: Constants and enum values. Prefer PascalCase for enum values.
- **File Names**: Match primary class name in PascalCase (e.g., `CpuState.cs`).

## Syntax Choices & C# 13 / .NET 9 Idioms

- **Implicit Usings & Nullable Reference Types**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`, `<Nullable>enable</Nullable>`). Treat warnings as errors where possible.
- **Primary Constructors**: Use primary constructors for immutability or simple dependency injection where clear.
  - Example: `public class MemoryBus(byte[] ram) { ... }`
- **Pattern Matching**: Prefer `switch` expressions and pattern matching for opcode decoding and bit manipulation.
  - Example:
    ```csharp
    byte status = flag switch {
        StatusFlag.Zero => (byte)(_status & 0x02),
        _ => 0
    };
    ```
- **Performance & Fixed Types**: Use explicit fixed-width integers (`byte` for 8-bit, `ushort` for 16-bit registers/addresses) over generic `int` for emulator register and memory operations.
- **Expression-bodied Members**: Use for single-line property getters or simple helper methods.

## Anti-Patterns

- **Do NOT use magic numbers for bitwise operations**: Define bitmasks as `const byte` or enum flags (e.g., `StatusFlags.ZeroBit = 1 << 1`).
- **Do NOT hide implicit type casting**: Always be explicit when casting uint/ushort to byte when performing wrapping 6502 arithmetic.
- **Do NOT throw generic `Exception`**: Throw specific exceptions (e.g., `InvalidOperationException` or custom `InvalidOpcodeException`).
