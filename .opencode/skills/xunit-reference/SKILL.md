---
name: xunit-reference
description: Minimal xUnit reference focused on short, precise, human-readable tests. Prefer direct tests with minimal setup, minimal indirection, and no unnecessary test abstractions.
---

# xUnit Testing Reference

## Core Rule

Tests are documentation.

A good test should be:

- Short
- Precise
- Direct
- Easy to understand without navigating elsewhere
- Focused on one behavior

Prefer repetition over indirection when repetition makes tests easier to read.

Do not create helpers, fixtures, base classes, data classes, or abstractions merely to reduce line count.

The test should make the behavior obvious:

```csharp
[Fact]
public void Add_TwoNumbers_ReturnsSum()
{
    var result = calculator.Add(2, 3);

    Assert.Equal(5, result);
}
```

Prefer this over hiding the setup or assertion behind helpers.

---

## 1. `[Fact]`

Use `[Fact]` when the test has one meaningful set of inputs.

```csharp
[Fact]
public void Add_TwoNumbers_ReturnsSum()
{
    Assert.Equal(5, calculator.Add(2, 3));
}
```

Skip only when there is a clear reason:

```csharp
[Fact(Skip = "Requires Windows")]
public void UsesWindowsRegistry() { }
```

---

## 2. `[Theory]`

Use `[Theory]` when several inputs test the **same behavior**.

```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(5, 4, 9)]
[InlineData(-1, 1, 0)]
public void Add_ReturnsExpectedResult(int a, int b, int expected)
{
    Assert.Equal(expected, calculator.Add(a, b));
}
```

Do not use `[Theory]` merely because inputs differ.

If the cases communicate different behavior, use separate `[Fact]` tests.

Prefer:

```csharp
[Fact]
public void Add_Zero_ReturnsOtherNumber()
{
    Assert.Equal(5, calculator.Add(5, 0));
}

[Fact]
public void Add_NegativeNumber_ReturnsDifference()
{
    Assert.Equal(2, calculator.Add(5, -3));
}
```

over one large theory containing unrelated cases.

---

## 3. Test Data

Prefer the simplest data source possible.

### `InlineData`

Default choice for small, readable values.

```csharp
[Theory]
[InlineData(0, false)]
[InlineData(1, true)]
[InlineData(-1, false)]
public void IsPositive_ReturnsExpected(int value, bool expected)
{
    Assert.Equal(expected, IsPositive(value));
}
```

### `MemberData`

Use only when `InlineData` cannot represent the data cleanly.

```csharp
public static TheoryData<byte, bool> Values => new()
{
    { 0x00, false },
    { 0x80, true }
};

[Theory]
[MemberData(nameof(Values))]
public void IsNegative_ReturnsExpected(byte value, bool expected)
{
    Assert.Equal(expected, IsNegative(value));
}
```

Prefer `TheoryData<T...>` over `IEnumerable<object[]>`.

### `ClassData`

Avoid unless the data is genuinely large or reusable.

Do not create a separate data class just to avoid a few `[InlineData]` lines.

---

## 4. Assertions

Expected value always comes first:

```csharp
Assert.Equal(expected, actual);
```

Never use `Assert.Equals()`.

### Common assertions

```csharp
Assert.Equal(expected, actual);
Assert.NotEqual(expected, actual);

Assert.True(condition);
Assert.False(condition);

Assert.Null(value);
Assert.NotNull(value);

Assert.Same(expected, actual);
Assert.NotSame(expected, actual);

Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Single(collection);

Assert.Contains(item, collection);
Assert.DoesNotContain(item, collection);

Assert.InRange(value, min, max);
Assert.NotInRange(value, min, max);
```

### Exceptions

```csharp
var exception = Assert.Throws<ArgumentNullException>(
    () => Method(null));

Assert.Equal("value", exception.ParamName);
```

For asynchronous code:

```csharp
await Assert.ThrowsAsync<InvalidOperationException>(
    () => MethodAsync());
```

---

## 5. Keep Setup Local

Prefer setup directly inside the test.

```csharp
[Fact]
public void Store_Value_CanBeRetrieved()
{
    var store = new Store();

    store.Set("name", "Bruno");

    Assert.Equal("Bruno", store.Get("name"));
}
```

Avoid moving simple setup into helpers:

```csharp
var store = CreateInitializedStore();
Assert.Equal("Bruno", store.Get("name"));
```

The second version forces the reader to navigate elsewhere to understand the test.

Use helpers only when they remove substantial, repeated noise without hiding the behavior being tested.

---

## 6. Keep Tests Independent

Each test should establish the state it needs.

Avoid:

- Shared mutable state
- Test ordering
- Global setup
- Static test state
- Tests depending on other tests

A test should be understandable and runnable by itself.

---

## 7. Fixtures

Fixtures are for **expensive shared resources**, not ordinary setup.

Good uses:

- Database
- Docker container
- External service
- Large immutable test environment

Bad use:

```csharp
public class CalculatorFixture
{
    public Calculator Calculator { get; } = new();
}
```

Just create the calculator in the test.

### Class fixture

```csharp
public class DatabaseFixture : IDisposable
{
    public Database Database { get; } = CreateDatabase();

    public void Dispose() => Database.Dispose();
}

public class UserTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture fixture;

    public UserTests(DatabaseFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void CreateUser_PersistsUser()
    {
        // ...
    }
}
```

Do not introduce fixtures to make test files shorter.

---

## 8. Test Class Organization

Group tests by the behavior they describe.

For a 6502 instruction:

```csharp
public class AdcTests
{
    [Fact]
    public void AddsAccumulatorAndOperand()
    {
        // ...
    }

    [Fact]
    public void SetsCarryWhenResultOverflowsByte()
    {
        // ...
    }

    [Fact]
    public void SetsZeroWhenResultIsZero()
    {
        // ...
    }
}
```

Prefer one cohesive test class over many tiny files.

A 500-line test file is not automatically a problem.

A 20-line test that requires opening five other files to understand it is.

Split a test class when it contains genuinely different concepts, not merely because the file became long.

---

## 9. Test Naming

Names should describe behavior.

Prefer:

```csharp
Lda_Immediate_LoadsAccumulator
Adc_WithCarry_SetsCarryFlag
Jmp_Absolute_SetsProgramCounter
```

Avoid:

```csharp
TestLda()
ShouldWork()
TestCase1()
Execute()
```

The name should help a failure explain itself.

---

## 10. 6502 Tests

6502 tests should make CPU state changes explicit.

Prefer:

```csharp
[Fact]
public void Lda_Immediate_LoadsAccumulator()
{
    cpu.Load(0xA9, 0x42);

    cpu.Step();

    Assert.Equal(0x42, cpu.A);
}
```

For flags:

```csharp
[Fact]
public void Lda_Zero_SetsZeroFlag()
{
    cpu.Load(0xA9, 0x00);

    cpu.Step();

    Assert.True(cpu.Status.HasFlag(Status.Zero));
}
```

When several flag combinations are the same behavior, use a theory:

```csharp
[Theory]
[InlineData(0x00, true)]
[InlineData(0x01, false)]
[InlineData(0x80, false)]
public void Lda_SetsZeroFlagCorrectly(byte value, bool zero)
{
    cpu.Load(0xA9, value);

    cpu.Step();

    Assert.Equal(zero, cpu.Status.HasFlag(Status.Zero));
}
```

Do not create a test framework inside the test framework.

Avoid elaborate builders, fixture hierarchies, generic test helpers, or shared setup unless the 6502 tests demonstrate a real need for them.

---

## 11. Parallelism

Tests should normally remain parallel-safe.

If tests require shared mutable resources, use xUnit collections or disable parallelization deliberately.

```csharp
[assembly: CollectionBehavior(DisableTestParallelization = true)]
```

Do not disable parallelism simply because tests are easier to write that way.

---

## 12. Output

Use `ITestOutputHelper` when diagnostic output is actually useful.

```csharp
public class Tests
{
    private readonly ITestOutputHelper output;

    public Tests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Test()
    {
        output.WriteLine("Debug information");
    }
}
```

Do not litter tests with logging.

---

## 13. Traits

Use traits only when they provide useful filtering.

```csharp
[Fact]
[Trait("Category", "Integration")]
public void DatabaseConnection_Works()
{
    // ...
}
```

Filter with:

```bash
dotnet test --filter "Category=Integration"
```

---

## Rules of Thumb

1. **Prefer `[Fact]` over abstractions.**
2. **Use `[Theory]` for genuinely equivalent cases.**
3. **Prefer `InlineData` for small datasets.**
4. **Keep setup inside the test.**
5. **Keep assertions directly visible.**
6. **Repeat a few lines rather than hiding them behind helpers.**
7. **Use fixtures only for expensive shared resources.**
8. **Do not mirror production folders just to organize tests.**
9. **Keep related tests together.**
10. **Optimize for the reader, not the line count.**
11. **A test should explain the behavior without requiring a debugger.**
12. **Do not build abstractions until repeated test code demonstrates that one is needed.**
13. **If removing an abstraction makes the test easier to understand, remove it.**