# MOS 6502 Emulator

[![Documentation](https://img.shields.io/badge/docs-GitHub_Pages-blue.svg)](https://bearingme.github.io/csharp_6502-emulator/)

A MOS 6502 microprocessor emulator written in C# targeting .NET 10 (`net10.0`).

📖 **Documentation**: [https://bearingme.github.io/csharp_6502-emulator/](https://bearingme.github.io/csharp_6502-emulator/)

## AI Usage Disclaimer

> **Important**: AI assistance in this repository is strictly restricted to **writing test suites, specifications, and documentation**. 
>
> All core emulator architecture, domain logic, and instruction implementations are designed and written by the author. AI is utilized solely as a verification harness and documentation assistant.

## Project Overview

This project is a study implementation of the classic 8-bit MOS Technology 6502 processor, structured with Domain-Driven Design (DDD) principles.

### Tech Stack
- **Language**: C# 14 / .NET 10 (`net10.0`)
- **Build Tool / SDK**: .NET SDK (`dotnet`)
- **Testing**: xUnit, FluentAssertions

## Architecture

The project follows a clean Domain, Application, and Infrastructure layout:

```
src/
├── Application/
│   └── Emulator.cs             # Application service: orchestration, program loading & step cycle
├── Domain/
│   ├── Entities/
│   │   ├── Addressing.cs       # 6502 addressing mode calculations
│   │   ├── Bus.cs              # 64 KiB memory bus
│   │   ├── Cpu.cs              # 6502 registers and flags state
│   │   └── Instructions.cs     # 6502 instruction execution logic
│   ├── Enums/
│   │   └── Status.cs           # Status flags (Carry, Zero, Interrupt, Decimal, Break, Overflow, Negative)
│   └── Objects/
│       ├── AddressingResult.cs # Result container for addressing calculations
│       └── InstructionResult.cs# Result container for instruction cycles
└── Infrastructure/
    └── Cli/
        └── Program.cs          # Functional test runner console host
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

### Clean
```bash
dotnet clean
```
