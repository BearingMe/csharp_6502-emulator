namespace Mos6502.Tests.Application.Emulator;

public class ExecutionPipelineTests
{
  [Fact]
  public void Step_NoOperand_ExecutesInstructionAndAdvancesPcByOne()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x8000, 0xE8); // INX
    var emulator = new Mos6502.Application.Emulator(bus);

    var cycles = emulator.Step();

    emulator.X.Should().Be(0x01);
    emulator.PC.Should().Be(0x8001);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_ByteOperand_ExecutesInstructionAndAdvancesPcByTwo()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x8000, 0xA9); // LDA Immediate
    bus.WriteByte(0x8001, 0x42);
    var emulator = new Mos6502.Application.Emulator(bus);

    var cycles = emulator.Step();

    emulator.A.Should().Be(0x42);
    emulator.PC.Should().Be(0x8002);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_WordOperand_ExecutesInstructionAndAdvancesPcByThree()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x8000, 0xAD); // LDA Absolute
    bus.WriteByte(0x8001, 0x34);
    bus.WriteByte(0x8002, 0x12);
    bus.WriteByte(0x1234, 0x99);
    var emulator = new Mos6502.Application.Emulator(bus);

    var cycles = emulator.Step();

    emulator.A.Should().Be(0x99);
    emulator.PC.Should().Be(0x8003);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Step_ThrowsException_OnUnimplementedOpcode()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x8000, 0xFF); // Unimplemented opcode
    var emulator = new Mos6502.Application.Emulator(bus);

    var act = () => emulator.Step();

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void LoadRom_WritesBytesSequentiallyToMemory()
  {
    var bus = new Bus();
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] rom = [0xA9, 0x10, 0x85, 0x20];

    emulator.LoadRom(rom, 0x8000);

    bus.ReadByte(0x8000).Should().Be(0xA9);
    bus.ReadByte(0x8001).Should().Be(0x10);
    bus.ReadByte(0x8002).Should().Be(0x85);
    bus.ReadByte(0x8003).Should().Be(0x20);
  }

  [Fact]
  public void Cycle_ExecutesSingleInstruction_WhenGivenInstructionCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xE8, // INX (2 cycles)
      0xE8  // INX (2 cycles)
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Cycle(2);

    emulator.X.Should().Be(0x01);
    emulator.PC.Should().Be(0x8001);
  }

  [Fact]
  public void Cycle_WithClockBudget_ExecutesInstructionsUntilBudgetReached()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA9, 0x05, // LDA #$05 (2 cycles)
      0xAA,       // TAX      (2 cycles)
      0xE8        // INX      (2 cycles)
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Cycle(6);

    emulator.A.Should().Be(0x05);
    emulator.X.Should().Be(0x06);
    emulator.PC.Should().Be(0x8004);
  }

  [Fact]
  public void LoadRom_ThrowsException_WhenRomExceedsAddressSpace()
  {
    var bus = new Bus();
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] oversizedRom = new byte[0x8001];

    var act = () => emulator.LoadRom(oversizedRom, 0x8000);

    act.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void Step_BranchTaken_ExecutesAndAdvancesPcToTarget()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA9, 0x80, // LDA #$80 (sets Negative flag)
      0x30, 0x04  // BMI +4 -> jumps to 0x8008
    ];
    emulator.LoadRom(program, 0x8000);

    var ldaCycles = emulator.Step();
    var branchCycles = emulator.Step();

    ldaCycles.Should().Be(2);
    branchCycles.Should().Be(3);
    emulator.PC.Should().Be(0x8008);
  }

  [Fact]
  public void Step_BranchNotTaken_AdvancesPcToNextInstruction()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA9, 0x01, // LDA #$01 (clears Negative flag)
      0x30, 0x04  // BMI +4 -> not taken
    ];
    emulator.LoadRom(program, 0x8000);

    var ldaCycles = emulator.Step();
    var branchCycles = emulator.Step();

    ldaCycles.Should().Be(2);
    branchCycles.Should().Be(2);
    emulator.PC.Should().Be(0x8004);
  }

  [Fact]
  public void Step_Dex_DecrementsXAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA2, 0x05, // LDX #$05
      0xCA        // DEX
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    emulator.X.Should().Be(0x04);
    emulator.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_Dey_DecrementsYAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA0, 0x05, // LDY #$05
      0x88        // DEY
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    emulator.Y.Should().Be(0x04);
    emulator.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_AslAccumulator_ShiftsLeftAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA9, 0x40, // LDA #$40
      0x0A        // ASL A
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    emulator.A.Should().Be(0x80);
    emulator.Status.HasFlag(Status.Negative).Should().BeTrue();
    emulator.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_AslAbsoluteX_ModifiesMemoryAndReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2004, 0x03);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA2, 0x04,       // LDX #$04
      0x1E, 0x00, 0x20  // ASL $2000,X
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    bus.ReadByte(0x2004).Should().Be(0x06);
    emulator.PC.Should().Be(0x8005);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Step_LsrAccumulator_ShiftsRightAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA9, 0x03, // LDA #$03
      0x4A        // LSR A
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    emulator.A.Should().Be(0x01);
    emulator.Status.HasFlag(Status.Carry).Should().BeTrue();
    emulator.Status.HasFlag(Status.Negative).Should().BeFalse();
    emulator.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_LsrAbsoluteX_ModifiesMemoryAndReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2004, 0x05);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA2, 0x04,       // LDX #$04
      0x5E, 0x00, 0x20  // LSR $2000,X
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    bus.ReadByte(0x2004).Should().Be(0x02);
    emulator.Status.HasFlag(Status.Carry).Should().BeTrue();
    emulator.PC.Should().Be(0x8005);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Step_RolAccumulator_RotatesLeftAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA9, 0x40, // LDA #$40
      0x2A        // ROL A
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    emulator.A.Should().Be(0x80);
    emulator.Status.HasFlag(Status.Negative).Should().BeTrue();
    emulator.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_RolAbsoluteX_ModifiesMemoryAndReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2004, 0x03);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA2, 0x04,       // LDX #$04
      0x3E, 0x00, 0x20  // ROL $2000,X
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    bus.ReadByte(0x2004).Should().Be(0x06);
    emulator.PC.Should().Be(0x8005);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Step_RorAccumulator_RotatesRightAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA9, 0x03, // LDA #$03
      0x6A        // ROR A
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    emulator.A.Should().Be(0x01);
    emulator.Status.HasFlag(Status.Carry).Should().BeTrue();
    emulator.Status.HasFlag(Status.Negative).Should().BeFalse();
    emulator.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_RorAbsoluteX_ModifiesMemoryAndReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2004, 0x05);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA2, 0x04,       // LDX #$04
      0x7E, 0x00, 0x20  // ROR $2000,X
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    bus.ReadByte(0x2004).Should().Be(0x02);
    emulator.Status.HasFlag(Status.Carry).Should().BeTrue();
    emulator.PC.Should().Be(0x8005);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Step_IncZeroPage_ModifiesMemoryAndReturnsFiveCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x0042, 0x05);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xE6, 0x42 // INC $42
    ];
    emulator.LoadRom(program, 0x8000);

    var cycles = emulator.Step();

    bus.ReadByte(0x0042).Should().Be(0x06);
    emulator.PC.Should().Be(0x8002);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Step_IncZeroPageX_ModifiesMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x0045, 0x10);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA2, 0x05, // LDX #$05
      0xF6, 0x40  // INC $40,X
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    bus.ReadByte(0x0045).Should().Be(0x11);
    emulator.PC.Should().Be(0x8004);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Step_IncAbsolute_ModifiesMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2000, 0x20);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xEE, 0x00, 0x20 // INC $2000
    ];
    emulator.LoadRom(program, 0x8000);

    var cycles = emulator.Step();

    bus.ReadByte(0x2000).Should().Be(0x21);
    emulator.PC.Should().Be(0x8003);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Step_IncAbsoluteX_ModifiesMemoryAndReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2004, 0x09);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA2, 0x04,       // LDX #$04
      0xFE, 0x00, 0x20  // INC $2000,X
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    bus.ReadByte(0x2004).Should().Be(0x0A);
    emulator.PC.Should().Be(0x8005);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Step_Pha_PushesAccumulatorAndAdvancesPcByOne()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA9, 0x33, // LDA #$33
      0x48        // PHA
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    bus.ReadByte(0x01FD).Should().Be(0x33);
    emulator.StackPointer.Should().Be(0xFC);
    emulator.PC.Should().Be(0x8003);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Step_Php_PushesStatusWithBreakAndUnusedBitsAndAdvancesPcByOne()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0x08 // PHP
    ];
    emulator.LoadRom(program, 0x8000);

    var cycles = emulator.Step();

    bus.ReadByte(0x01FD).Should().Be((u8)(Status.Interrupt | Status.Break | Status.Unused));
    emulator.StackPointer.Should().Be(0xFC);
    emulator.PC.Should().Be(0x8001);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Step_Pla_PullsAccumulatorAndAdvancesPcByOne()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x01FD, 0x77);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA2, 0xFC, // LDX #$FC
      0x9A,       // TXS
      0x68        // PLA
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    emulator.Step();
    var cycles = emulator.Step();

    emulator.A.Should().Be(0x77);
    emulator.StackPointer.Should().Be(0xFD);
    emulator.PC.Should().Be(0x8004);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Step_Plp_PullsStatusAndAdvancesPcByOne()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x01FD, (u8)(Status.Carry | Status.Zero));
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA2, 0xFC, // LDX #$FC
      0x9A,       // TXS
      0x28        // PLP
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    emulator.Step();
    var cycles = emulator.Step();

    emulator.Status.HasFlag(Status.Carry).Should().BeTrue();
    emulator.Status.HasFlag(Status.Zero).Should().BeTrue();
    emulator.StackPointer.Should().Be(0xFD);
    emulator.PC.Should().Be(0x8004);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Step_JmpAbsolute_SetsProgramCounterAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0x4C, 0x50, 0x90 // JMP $9050
    ];
    emulator.LoadRom(program, 0x8000);

    var cycles = emulator.Step();

    emulator.PC.Should().Be(0x9050);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Step_JmpIndirect_SetsProgramCounterAndReturnsFiveCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x1000, 0x50);
    bus.WriteByte(0x1001, 0x90);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0x6C, 0x00, 0x10 // JMP ($1000)
    ];
    emulator.LoadRom(program, 0x8000);

    var cycles = emulator.Step();

    emulator.PC.Should().Be(0x9050);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Step_JmpIndirect_WithPageBoundaryBug_SetsProgramCounterAndReturnsFiveCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x10FF, 0x50);
    bus.WriteByte(0x1000, 0x90);
    bus.WriteByte(0x1100, 0x22); // CMOS high byte, ignored on NMOS
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0x6C, 0xFF, 0x10 // JMP ($10FF)
    ];
    emulator.LoadRom(program, 0x8000);

    var cycles = emulator.Step();

    emulator.PC.Should().Be(0x9050);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Step_Rti_PullsStatusAndProgramCounterAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x01FB, (u8)(Status.Carry | Status.Zero));
    bus.WriteByte(0x01FC, 0x20); // PC Low
    bus.WriteByte(0x01FD, 0x40); // PC High
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA2, 0xFA, // LDX #$FA
      0x9A,       // TXS
      0x40        // RTI
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    emulator.Step();
    var cycles = emulator.Step();

    emulator.PC.Should().Be(0x4020);
    emulator.StackPointer.Should().Be(0xFD);
    emulator.Status.HasFlag(Status.Carry).Should().BeTrue();
    emulator.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(6);
  }

  [Fact]
  public void Step_DecZeroPage_DecrementsMemoryAndReturnsFiveCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x0042, 0x05);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xC6, 0x42 // DEC $42
    ];
    emulator.LoadRom(program, 0x8000);

    var cycles = emulator.Step();

    bus.ReadByte(0x0042).Should().Be(0x04);
    emulator.PC.Should().Be(0x8002);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Step_DecAbsoluteX_DecrementsMemoryAndReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2004, 0x01);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xA2, 0x04,       // LDX #$04
      0xDE, 0x00, 0x20  // DEC $2000,X
    ];
    emulator.LoadRom(program, 0x8000);

    emulator.Step();
    var cycles = emulator.Step();

    bus.ReadByte(0x2004).Should().Be(0x00);
    emulator.PC.Should().Be(0x8005);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Step_JsrAndRts_ExecutesSubroutineAndReturns()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0x20, 0x05, 0x80, // JSR $8005 (3 bytes, PC -> $8003 before jumping)
      0xEA,             // NOP (target of RTS: $8003)
      0xEA,             // NOP ($8004)
      0xE8,             // Subroutine: INX ($8005)
      0x60              // Subroutine: RTS ($8006)
    ];
    emulator.LoadRom(program, 0x8000);

    var jsrCycles = emulator.Step(); // JSR $8005
    jsrCycles.Should().Be(6);
    emulator.PC.Should().Be(0x8005);

    var inxCycles = emulator.Step(); // INX
    inxCycles.Should().Be(2);
    emulator.X.Should().Be(0x01);

    var rtsCycles = emulator.Step(); // RTS
    rtsCycles.Should().Be(6);
    emulator.PC.Should().Be(0x8003);

    var nopCycles = emulator.Step(); // NOP at $8003
    nopCycles.Should().Be(2);
    emulator.PC.Should().Be(0x8004);
  }

  [Fact]
  public void Step_Nop_AdvancesPcByOneAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0xEA // NOP
    ];
    emulator.LoadRom(program, 0x8000);

    var cycles = emulator.Step();

    emulator.PC.Should().Be(0x8001);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_Brk_PushesFrameSetsInterruptFlagAndJumpsToVector_ReturningSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0xFFFE, 0x00);
    bus.WriteByte(0xFFFF, 0x90);
    var emulator = new Mos6502.Application.Emulator(bus);
    byte[] program =
    [
      0x00 // BRK
    ];
    emulator.LoadRom(program, 0x8000);

    var cycles = emulator.Step();

    emulator.PC.Should().Be(0x9000);
    emulator.Status.HasFlag(Status.Interrupt).Should().BeTrue();
    cycles.Should().Be(7);
  }
}
