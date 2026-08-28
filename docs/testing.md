# Testing Strategy

## Stack

- **Framework**: xUnit (`2.9.3`)
- **Assertions**: FluentAssertions (`8.10.0`)
- **Mocking**: No mocking frameworks. Tests use real lightweight `Bus`, `Cpu`, `Addressing`, `Instructions`, and `Emulator` instances.
- **Target**: .NET 10 (`net10.0`)

See `AGENTS.md` for test execution commands.

---

## Testing Principles

Tests are executable specifications.

They must be:

- **Minimal**: contain only what is necessary to express the behavior.
- **Human-readable**: a reader should understand the scenario and expected result immediately.
- **Specification-driven**: test what the 6502 is required to do, not how the emulator happens to implement it.
- **Deterministic**: identical inputs produce identical results.
- **Independent**: tests must not share mutable state or depend on execution order.
- **Direct**: avoid unnecessary fixtures, builders, base classes, or indirection.

Prefer readable repetition over helpers that obscure important setup or assertions.

---

## Test Organization & Hierarchy

Tests are placed in `tests/mos6502.Tests/` and strictly mirror the `src/` directory layout:

```
tests/mos6502.Tests/
├── Domain/
│   └── Entities/
│       ├── AddressingEntity/
│       │   └── AddressingTests.cs           # Addressing calculations & page boundary crossings
│       ├── BusEntity/
│       │   └── BusTests.cs                  # Memory read/write and 16-bit word handling
│       ├── CpuEntity/
│       │   └── CpuTests.cs                  # Register initialization & Reset state
│       └── InstructionsEntity/
│           ├── AdcTests.cs                  # Opcode-specific instruction tests
│           ├── SbcTests.cs
│           ├── LdaTests.cs
│           ├── StaTests.cs
│           └── ... (one per instruction family)
└── Application/
    └── Emulator/
        ├── ExecutionPipelineTests.cs        # Opcode fetch/decode/execute dispatch via Step()
        └── Functional/
            ├── FunctionalArithmeticStackPipelineTests.cs # Multi-instruction workflows
            ├── FunctionalBranchingLoopTests.cs
            ├── FunctionalIndexedIndirectAddressingTests.cs
            ├── FunctionalIndirectJmpPageWrapTests.cs
            ├── FunctionalInterruptLifecycleTests.cs
            ├── FunctionalPageCrossingBranchTests.cs
            └── FunctionalSubroutineLifecycleTests.cs
```

---

## Test Levels

### 1. Domain Unit Tests (`tests/mos6502.Tests/Domain/Entities/`)

Test domain components in isolation using real domain entities:

- **Addressing Tests**: Verify effective address computation, zero page wrap-around, and cycle penalties for page crossings (`AbsoluteX`, `AbsoluteY`, `IndirectIndexed`, `Indirect`).
- **Instruction Tests**: Verify ALU arithmetic, boolean operations, shifts, status flags, and cycle counts for individual instruction modes (e.g. `AdcImmediate`, `SbcAbsolute`).
- **Bus & CPU Tests**: Verify memory bounds, 16-bit word byte order, register state, and reset vector handling.

Example unit test:

```csharp
[Fact]
public void Adc_Immediate_AddsWithCarryAndSetsFlags()
{
  var bus = new Bus();
  var cpu = new Emulator(bus);
  cpu.A = 0x20;

  var cycles = cpu.AdcImmediate(0x15);

  cpu.A.Should().Be(0x35);
  cpu.Status.HasFlag(Status.Carry).Should().BeFalse();
  cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
  cycles.Should().Be(2);
}
```

### 2. Application Pipeline Tests (`tests/mos6502.Tests/Application/Emulator/`)

Verify that `Emulator.Step()` fetches the opcode from memory at `PC`, increments `PC`, dispatches to the correct instruction handler, and returns the total cycle count for that step.

Example pipeline test:

```csharp
[Fact]
public void Step_LdaImmediate_AdvancesPcAndReturnsCycles()
{
  var bus = new Bus();
  var emulator = new Emulator(bus);
  emulator.LoadRom([0xA9, 0x42], 0x8000);
  emulator.PC = 0x8000;

  var cycles = emulator.Step();

  emulator.A.Should().Be(0x42);
  emulator.PC.Should().Be(0x8002);
  cycles.Should().Be(2);
}
```

### 3. Multi-Instruction Functional Tests (`tests/mos6502.Tests/Application/Emulator/Functional/`)

Verify full multi-instruction sequences running consecutively through `Emulator.Step()`. These test complex interactions such as:
- Subroutine lifecycles (`JSR` → stack push → execution → `RTS` → stack pop → return address continuation).
- Interrupt lifecycles (`BRK` / IRQ → status push → vector jump → `RTI` return).
- Loops with conditional branches and accumulator decrementing.
- Indexed indirect memory pointer pipelines.

### 4. Binary Functional ROM Runner (`src/Infrastructure/Cli/Program.cs`)

Executes complete binary test ROMs (such as `nestest.nes`) loaded into 64 KiB RAM. Run via:

```bash
dotnet run --project src/mos6502.csproj
```

---

## Unit Test Conventions

### Naming

Test method names describe the operation, scenario, and expected observable behavior:

`[Operation]_[Scenario]_[ExpectedBehavior]`

Examples:

```csharp
Adc_Immediate_AddsValuesWithoutCarry
Sbc_SubtractsValues_AndUpdatesFlags_WithInitialCarry
Jmp_Indirect_WrapsAddressAcrossPageBoundary
Sta_AbsoluteX_AppliesXOffsetAndWritesToMemory
```

### Assertions

Assert all relevant observable state affected by the behavior under test:

- Result registers (`A`, `X`, `Y`, `PC`, `StackPointer`)
- Modified status flags (`Carry`, `Zero`, `Overflow`, `Negative`, `Interrupt`, etc.)
- Memory contents at target addresses
- Returned cycle counts

```csharp
[Fact]
public void Sbc_ZeroPage_ReadsOperandAndReturnsThreeCycles()
{
  var bus = new Bus();
  bus.WriteByte(0x0042, 0x15);
  var cpu = new Emulator(bus);
  cpu.LdaImmediate(0xFF);
  cpu.AdcImmediate(0x01); // Set Carry flag
  cpu.LdaImmediate(0x35);

  var cycles = cpu.SbcZeroPage(0x42);

  cpu.A.Should().Be(0x20);
  cycles.Should().Be(3);
}
```

---

## Data-Driven Tests

Use `[Theory]` with `[InlineData]` when multiple inputs test the same rule:

```csharp
[Theory]
[InlineData(0x50, 0x10, 0x40, true, false, false, false)]
[InlineData(0x50, 0x50, 0x00, true, true, false, false)]
[InlineData(0x50, 0x70, 0xE0, false, false, true, false)]
[InlineData(0x50, 0x90, 0xC0, false, false, true, true)]
[InlineData(0xD0, 0x70, 0x60, true, false, false, true)]
public void Sbc_SubtractsValues_AndUpdatesFlags_WithInitialCarry(
  u8 initialA,
  u8 operand,
  u8 expectedA,
  bool carry,
  bool zero,
  bool negative,
  bool overflow)
{
  var bus = new Bus();
  var cpu = new Emulator(bus);
  cpu.LdaImmediate(0xFF);
  cpu.AdcImmediate(0x01); // Set Carry flag

  cpu.LdaImmediate(initialA);
  var cycles = cpu.SbcImmediate(operand);

  cpu.A.Should().Be(expectedA);
  cpu.Status.HasFlag(Status.Carry).Should().Be(carry);
  cpu.Status.HasFlag(Status.Zero).Should().Be(zero);
  cpu.Status.HasFlag(Status.Negative).Should().Be(negative);
  cpu.Status.HasFlag(Status.Overflow).Should().Be(overflow);
  cycles.Should().Be(2);
}
```

---

## What Not to Test

Do not test:
- .NET standard library or C# runtime behaviors.
- Trivial auto-properties without custom logic.
- Private internal fields or method call order.
- Mock objects or simulated interfaces.

Always test observable MOS 6502 behavior through the public domain and application APIs.
