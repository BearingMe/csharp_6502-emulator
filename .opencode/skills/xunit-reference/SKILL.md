---
name: xunit-reference
description: Comprehensive reference for xUnit testing framework in .NET. Covers project setup, test attributes ([Fact], [Theory]), data-driven testing ([InlineData], [MemberData], [ClassData], TheoryData<T>), assertions, fixtures (Class, Collection, Assembly, IAsyncLifetime), parallelism, diagnostic output, traits, and dynamic skipping. Use whenever writing, reading, or refactoring xUnit tests in .NET.
---

# xUnit Framework Reference

A project-agnostic, condensed reference for xUnit testing in .NET.

---

## 1. Project Setup & Package Dependencies

```bash
# Create test project
dotnet new xunit -o ./tests/MyApp.Tests -n MyApp.Tests

# Essential packages
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk

# Optional utility packages
dotnet add package coverlet.collector   # Code coverage
```

Recommended project settings (`.csproj`):
```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <IsPackable>false</IsPackable>
</PropertyGroup>
```

---

## 2. Defining Tests

### `[Fact]` — Single Unparameterized Test

```csharp
using Xunit;

public class CalculatorTests
{
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        var result = 1 + 2;
        Assert.Equal(3, result);
    }

    [Fact(Skip = "Reason for skipping")]
    public void SkippedTest() { }

    [Fact(DisplayName = "Custom test display name in runner")]
    public void CustomDisplayNameTest() { }
}
```

### `[Theory]` — Parameterized Test

```csharp
public class MathTests
{
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 4, 9)]
    [InlineData(-1, 1, 0)]
    public void Add_MultipleInputs_ReturnsExpected(int a, int b, int expected)
    {
        Assert.Equal(expected, a + b);
    }
}
```

---

## 3. Data-Driven Test Sources

### `InlineData` — Constant literal values

```csharp
[Theory]
[InlineData("hello", 5)]
[InlineData("", 0)]
public void StringLength(string input, int expectedLength)
{
    Assert.Equal(expectedLength, input.Length);
}
```

### `MemberData` — Static property or method

```csharp
public class DataDrivenTests
{
    // Strongly typed TheoryData<T...> (PREFERRED over raw object[] arrays)
    public static TheoryData<int, int, int> AdditionData => new()
    {
        { 18, 24, 42 },
        { 6, 7, 13 },
    };

    [Theory]
    [MemberData(nameof(AdditionData))]
    public void Add_WithTheoryData(int a, int b, int expected)
    {
        Assert.Equal(expected, a + b);
    }

    // Classic IEnumerable<object[]> pattern
    public static IEnumerable<object[]> RawData()
    {
        yield return new object[] { 10, 20, 30 };
        yield return new object[] { 1, 1, 2 };
    }

    [Theory]
    [MemberData(nameof(RawData))]
    public void Add_WithRawData(int a, int b, int expected)
    {
        Assert.Equal(expected, a + b);
    }
}
```

### `ClassData` — Separate data class

```csharp
public class CustomTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 10, true };
        yield return new object[] { -5, false };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Theory]
[ClassData(typeof(CustomTestData))]
public void IsPositive_ValidatesNumber(int input, bool expected)
{
    Assert.Equal(expected, input > 0);
}
```

---

## 4. Assertions (`Assert` API)

### Equality & Identity
```csharp
Assert.Equal(expected, actual);            // Value equality
Assert.NotEqual(expected, actual);
Assert.StrictEqual(expected, actual);      // Strict type & value match
Assert.Same(expectedReference, actual);    // Reference equality (object.ReferenceEquals)
Assert.NotSame(expectedReference, actual);
```

> **CRITICAL RULE**: ALWAYS use `Assert.Equal(expected, actual)`. NEVER use `Assert.Equals()` (which calls `object.Equals()` and always throws or fails statically). The expected value must ALWAYS be the first argument.

### Booleans & Nullability
```csharp
Assert.True(condition);
Assert.True(condition, "Optional user message on failure");
Assert.False(condition);
Assert.Null(objectInstance);
Assert.NotNull(objectInstance);
```

### Type Assertions
```csharp
T instance = Assert.IsType<T>(objectInstance);        // Exact type match
T derived  = Assert.IsAssignableFrom<T>(objectInstance); // Type or subtype match
```

### Strings
```csharp
Assert.Equal("expected", str, ignoreCase: false);
Assert.StartsWith("prefix", str);
Assert.EndsWith("suffix", str);
Assert.Contains("sub", str);
Assert.DoesNotContain("sub", str);
Assert.Matches(@"^\d{3}-\d{2}-\d{4}$", str);          // Regex match
Assert.DoesNotMatch(@"\s+", str);
Assert.Empty(str);
Assert.NotEmpty(str);
```

### Collections
```csharp
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Single(collection);                           // Asserts exactly 1 element
Assert.Single(collection, item => item.Id == 1);     // Asserts exactly 1 element matching predicate
Assert.Contains(item, collection);
Assert.DoesNotContain(item, collection);
Assert.Equal(expectedList, actualList);              // Element-wise equality & order match
Assert.Equivalent(expectedSet, actualSet);           // Element equality ignoring order

// Test every element meets a predicate
Assert.All(collection, item => Assert.NotNull(item));

// Element-by-element structural assertions
Assert.Collection(collection,
    element0 => Assert.Equal("first", element0),
    element1 => Assert.Equal("second", element1)
);

Assert.InRange(value, lowInclusive, highInclusive);
Assert.NotInRange(value, lowInclusive, highInclusive);
```

### Exceptions
```csharp
// Synchronous exception assertion (returns exception for further checks)
var ex = Assert.Throws<ArgumentNullException>(() => MethodThatThrows(null));
Assert.Equal("paramName", ex.ParamName);

// Async exception assertion
var asyncEx = await Assert.ThrowsAsync<InvalidOperationException>(
    async () => await AsyncMethodThatThrows()
);
```

### Events & Property Notifications
```csharp
// Assert event raised
Assert.Raises<CustomEventArgs>(
    handler => publisher.CustomEvent += handler,
    handler => publisher.CustomEvent -= handler,
    () => publisher.DoSomething()
);

// Assert INotifyPropertyChanged
Assert.PropertyChanged(notifierObj, nameof(notifierObj.Property), () => {
    notifierObj.Property = "NewValue";
});
```

---

## 5. Shared Context & Fixtures Lifecycle

xUnit intentionally omits `[SetUp]` / `[TearDown]` attributes. It uses standard C# lifecycle mechanisms.

### Per-Test Lifecycle (Constructor + `IDisposable` / `IAsyncLifetime`)

A **new instance** of the test class is created for **every test method execution**.

```csharp
public class PerTestLifecycleTests : IDisposable, IAsyncLifetime
{
    public PerTestLifecycleTests()
    {
        // 1. Synchronous setup runs before EVERY test
    }

    public async Task InitializeAsync()
    {
        // 2. Async setup runs before EVERY test
        await Task.Yield();
    }

    [Fact]
    public void TestOne() { }

    public async Task DisposeAsync()
    {
        // 3. Async teardown runs after EVERY test
        await Task.Yield();
    }

    public void Dispose()
    {
        // 4. Synchronous teardown runs after EVERY test
    }
}
```

### Class Fixture — Shared across single test class

Resource is instantiated **once** before any test in the class runs, and cleaned up after all tests finish.

```csharp
public class SharedResourceFixture : IDisposable, IAsyncLifetime
{
    public string ConnectionString { get; private set; } = "";

    public async Task InitializeAsync()
    {
        ConnectionString = "Initialized";
        await Task.Delay(10);
    }

    public Task DisposeAsync() => Task.CompletedTask;
    public void Dispose() { }
}

public class ClassFixtureTests : IClassFixture<SharedResourceFixture>
{
    private readonly SharedResourceFixture _fixture;

    public ClassFixtureTests(SharedResourceFixture fixture)
    {
        _fixture = fixture; // Injected by xUnit
    }

    [Fact]
    public void TestUsingFixture()
    {
        Assert.Equal("Initialized", _fixture.ConnectionString);
    }
}
```

### Collection Fixture — Shared across multiple test classes

Share state across distinct test classes without re-initializing expensive setup.

```csharp
// Step 1: Create fixture class
public class DatabaseFixture : IDisposable
{
    public DatabaseFixture() { /* Expensive DB spin up */ }
    public void Dispose() { /* Clean up */ }
}

// Step 2: Define collection definition (marker class)
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // Interface & attribute applied here; no code needed in body
}

// Step 3: Decorate test classes to join the collection
[Collection("Database collection")]
public class UserTests
{
    private readonly DatabaseFixture _fixture;
    public UserTests(DatabaseFixture fixture) => _fixture = fixture;
}

[Collection("Database collection")]
public class ProductTests
{
    private readonly DatabaseFixture _fixture;
    public ProductTests(DatabaseFixture fixture) => _fixture = fixture;
}
```

### Assembly Fixture — Shared globally across the entire test assembly

```csharp
[assembly: AssemblyFixture(typeof(GlobalTestEnvironmentFixture))]

public class GlobalTestEnvironmentFixture : IDisposable
{
    public GlobalTestEnvironmentFixture() { /* Global setup */ }
    public void Dispose() { /* Global teardown */ }
}

// Inject in any test class in the assembly:
public class AnyTestClass
{
    public AnyTestClass(GlobalTestEnvironmentFixture globalFixture) { }
}
```

### Summary of Lifecycles

| Scope | Mechanism | Instantiated | Use Case |
|---|---|---|---|
| Per Test | Constructor / `IDisposable` / `IAsyncLifetime` | N times (1 per test) | Isolated state, per-test mocks |
| Per Class | `IClassFixture<T>` | 1 per class | Expensive setup shared within class |
| Per Collection | `ICollectionFixture<T>` + `[Collection]` | 1 per collection | DB / Docker container shared across classes |
| Per Assembly | `[assembly: AssemblyFixture(typeof(T))]` | 1 per assembly run | Global environment setup |

---

## 6. Parallel Execution Control

- By default, xUnit runs **test classes in parallel** (each class is placed in its own default test collection).
- Tests **within the same class** run sequentially.
- Test classes assigned to the **same `[Collection("name")]`** run sequentially relative to each other.

To disable parallel execution assembly-wide, create an `AssemblyInfo.cs` or set attribute:
```csharp
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
```

Or configure max parallel threads:
```csharp
[assembly: CollectionBehavior(MaxParallelThreads = 4)]
```

---

## 7. Diagnostic Output (`ITestOutputHelper`)

Standard `Console.WriteLine` output is captured and hidden by xUnit runner. Use `ITestOutputHelper` via constructor injection to write output linked to test results.

```csharp
using Xunit;
using Xunit.Abstractions;

public class OutputTests
{
    private readonly ITestOutputHelper _output;

    public OutputTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestWithLogging()
    {
        _output.WriteLine("Step 1: Initializing value");
        var val = 42;
        _output.WriteLine("Step 2: Value is {0}", val);
        Assert.Equal(42, val);
    }
}
```

---

## 8. Traits & Filtering

Use `[Trait("Category", "Value")]` to tag tests for command-line filtering.

```csharp
public class CategorizedTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void FastUnitTest() { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public void IntegrationTest() { }
}
```

Filter execution via CLI:
```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration&Priority=High"
dotnet test --filter "Category!=Integration"
```

---

## 9. Dynamic Skipping (Custom Attributes)

Extend `FactAttribute` or `TheoryAttribute` to conditionally skip tests at runtime based on environment.

```csharp
public sealed class WindowsOnlyFactAttribute : FactAttribute
{
    public WindowsOnlyFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            Skip = "Test runnable only on Windows platform.";
        }
    }
}

public class PlatformDependentTests
{
    [WindowsOnlyFact]
    public void RegistryTest() { }
}
```

---

## 10. Test Runner Configuration (`xunit.runner.json`)

Add an `xunit.runner.json` file at the root of the test project to configure execution:

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeAssembly": true,
  "parallelizeTestCollections": true,
  "maxParallelThreads": 0,
  "methodDisplay": "method",
  "methodDisplayOptions": "replaceUnderscoreWithSpace,useOperatorMonikers"
}
```

Ensure file is copied to output directory in `.csproj`:
```xml
<ItemGroup>
  <Content Include="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```
