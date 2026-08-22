# Architecture

MOS 6502 emulator implemented in .NET 10 (C# 14).

The architecture prioritizes correctness, simplicity, locality, and low coupling over abstraction or predefined organization.

## Core Principles

### Specification First

The 6502 hardware specification is the source of truth.

Implementation decisions must preserve the behavior defined by the specification.

When implementation and specification disagree, the implementation is wrong.

### Simplicity First

Do not introduce abstractions until the implementation demonstrates a real need for one.

Prefer:

- Direct method calls
- Concrete types
- Small cohesive components
- Local state
- Simple data structures
- Explicit control flow

Avoid:

- Unnecessary interfaces
- Dependency injection
- Factories
- Generic abstractions
- Deep inheritance
- Wrapper types without meaningful behavior
- Indirection created solely for organization

### Low Coupling

Components should depend only on what they actually need.

Do not pass large objects through multiple layers merely because one piece of information is required.

Adding a new instruction should not require threading dependencies through unrelated components.

### Single Source of Truth

CPU state belongs to the CPU.

This includes:

- `A`
- `X`
- `Y`
- `PC`
- `SP`
- Processor status flags

Memory belongs to the memory system.

Do not duplicate CPU or memory state in other components.

### Behavior Over Structure

Organize code around meaningful domain concepts, not arbitrary file sizes or symmetry.

Do not create classes, interfaces, namespaces, or layers merely to make the project look organized.

A large cohesive type is preferable to unnecessary fragmentation.

Refactor when repeated code demonstrates a real shared concept.

---

## Domain Responsibilities

### CPU

The CPU represents the processor and owns its state.

It is responsible for behavior intrinsic to the 6502, including:

- Register manipulation
- Status flags
- Arithmetic and logic operations
- Stack behavior
- Program counter behavior
- Instruction execution

The CPU must not depend on application or CLI concerns.

### Memory

The memory system owns addressable memory and memory access.

It is responsible for:

- Reading memory
- Writing memory
- Address handling
- The 6502 address space

It must not contain CPU instruction semantics.

### Addressing Modes

Addressing behavior interprets instruction operands according to the 6502 specification.

It determines things such as:

- Operand location
- Effective address
- Operand value

It must not contain instruction-specific behavior.

### Instructions

Instruction behavior implements the semantics defined by the 6502 specification.

Instructions should reuse addressing behavior rather than duplicate address calculation.

Instruction implementations should remain small and explicit.

### Application

Application-level code coordinates the emulator as a whole.

It may handle:

- Program loading
- Reset
- Execution loops
- CLI interaction
- High-level orchestration

Application code must not contain CPU semantics that belong to the domain.

---

## Dependency Rules

The dependency direction is:

<code>
Application
    ↓
Domain
</code>

The domain must not depend on application or CLI concerns.

Within the domain, dependencies should be kept as narrow as possible.

For example, code that only needs memory access should depend on memory access, not on the entire CPU.

Avoid dependency chains where a feature requires passing a reference through unrelated components.

The exact decomposition may change as implementation experience reveals better boundaries.

---

## Execution Model

A CPU execution cycle conceptually follows:

<code>
Fetch → Decode → Execute → Advance
</code>

The implementation may distribute these responsibilities across different components.

The architecture must not require a specific component to own a particular stage.

What matters is that observable execution matches the 6502 specification.

---

## Testing

Tests are executable specifications.

They verify observable behavior against the 6502 specification rather than implementation details.

Tests should:

- Use real lightweight domain objects
- Keep setup local and explicit
- Test behavior rather than implementation
- Minimize indirection
- Remain readable without navigating through helpers
- Cover meaningful specification edge cases

Do not create production abstractions solely to make tests easier to organize.

If production code is difficult to test, first question the production design rather than automatically adding test infrastructure.

---

## Fixed-Width Types

Use types that accurately represent the 6502 architecture:

- `byte` for 8-bit values
- `ushort` for 16-bit addresses
- Explicit conversions when arithmetic crosses width boundaries

Custom fixed-width value types may be introduced when they provide meaningful domain behavior or prevent real classes of errors.

Do not create wrappers merely for organizational consistency.

---

## External Dependencies

Core CPU and memory logic should use only the .NET standard library.

Third-party dependencies must not be introduced into core domain logic without a concrete reason.

Application and tooling code may have additional dependencies where appropriate.

---

## Future Changes

Do not design future architecture in advance.

Introduce abstractions only when actual requirements justify them.

Potential future concerns such as:

- Memory-mapped I/O
- Cartridges
- Peripheral devices
- Alternative memory implementations
- Instruction dispatch optimization

should be addressed when they become real requirements.

Do not add interfaces or abstraction layers for hypothetical future implementations.

---

## Architectural Decision Rule

When choosing between designs, prefer the one that:

1. Has fewer moving parts.
2. Has fewer dependencies.
3. Makes 6502 behavior easier to see.
4. Requires less navigation to understand.
5. Is easier to test directly.
6. Is easier to change when the specification reveals a mistake.

**The architecture exists to make the emulator easier to understand and change. The emulator does not exist to justify the architecture.**