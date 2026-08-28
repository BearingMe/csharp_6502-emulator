# MOS 6502 Emulator

[![Documentation](https://img.shields.io/badge/docs-GitHub_Pages-blue.svg)](https://bearingme.github.io/csharp_6502-emulator/)

A MOS 6502 microprocessor emulator written in C# 14 targeting .NET 10 (`net10.0`).

📖 **Documentation**: [https://bearingme.github.io/csharp_6502-emulator/](https://bearingme.github.io/csharp_6502-emulator/)

## AI Usage Disclaimer

> **Important**: AI assistance in this repository is strictly restricted to **writing test suites, specifications, and documentation**. 
>
> All core emulator architecture, domain logic, and instruction implementations are designed and written by the author. AI is utilized solely as a verification harness and documentation assistant.

## Project Overview

This project is an emulator for the classic 8-bit MOS Technology 6502 processor, implemented in modern C# (.NET 10) with Domain-Driven Design (DDD) principles, low coupling, and direct constructor composition.

### Tech Stack
- **Language**: C# 14 / .NET 10 (`net10.0`)
- **Build Tool / SDK**: .NET SDK (`dotnet`)
- **Testing**: xUnit, FluentAssertions

## Architecture & Layout

The codebase consists of a single solution (`mos6502.slnx`) containing two projects:
1. `src/mos6502.csproj`: Core emulator library and CLI host.
2. `tests/mos6502.Tests/mos6502.Tests.csproj`: Comprehensive test suite mirroring `src/`.

```
src/
├── Domain/
│   ├── Entities/
│   │   ├── Addressing.cs       # 6502 addressing mode calculations
│   │   ├── Bus.cs              # 64 KiB addressable memory bus
│   │   ├── Cpu.cs              # 6502 registers, stack pointer, and status flags
│   │   └── Instructions.cs     # 6502 ALU, stack, branching, and memory instructions
│   ├── Enums/
│   │   └── Status.cs           # Status flags (Carry, Zero, Interrupt, Decimal, Break, Overflow, Negative)
│   └── Objects/
│       ├── AddressingResult.cs # Result container for effective address calculations
│       └── InstructionResult.cs# Result container for instruction cycle counts
├── Application/
│   └── Emulator.cs             # Application orchestrator: ROM loader, Reset, and Step() dispatch
└── Infrastructure/
    └── Cli/
        ├── Program.cs          # Functional test runner console host
        └── assets/roms/        # Bundled test ROMs (e.g., nestest.nes)
```

## Building and Testing

### Build
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Run Functional Test Runner (nestest)
Executes all official MOS 6502 instructions against the bundled `nestest.nes` ROM:
```bash
dotnet run --project src/mos6502.csproj
```

### Clean
```bash
dotnet clean
```
