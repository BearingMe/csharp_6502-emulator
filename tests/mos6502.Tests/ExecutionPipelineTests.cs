namespace mos6502.Tests;

public class ExecutionPipelineTests
{
  [Fact]
  public void Step_NoOperand_ExecutesInstructionAndAdvancesPcByOne()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x8000, 0xE8); // INX
    var cpu = new Emulator(bus);

    var cycles = cpu.Step();

    cpu.X.Should().Be(0x01);
    cpu.PC.Should().Be(0x8001);
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
    var cpu = new Emulator(bus);

    var cycles = cpu.Step();

    cpu.A.Should().Be(0x42);
    cpu.PC.Should().Be(0x8002);
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
    var cpu = new Emulator(bus);

    var cycles = cpu.Step();

    cpu.A.Should().Be(0x99);
    cpu.PC.Should().Be(0x8003);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Step_ThrowsException_OnUnimplementedOpcode()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x8000, 0xFF); // Unimplemented opcode
    var cpu = new Emulator(bus);

    var act = () => cpu.Step();

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void LoadRom_WritesBytesSequentiallyToMemory()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    byte[] rom = [0xA9, 0x10, 0x85, 0x20];

    cpu.LoadRom(rom, 0x8000);

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
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xE8, // INX (2 cycles)
      0xE8  // INX (2 cycles)
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Cycle(2);

    cpu.X.Should().Be(0x01);
    cpu.PC.Should().Be(0x8001);
  }

  [Fact]
  public void Cycle_WithClockBudget_ExecutesInstructionsUntilBudgetReached()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA9, 0x05, // LDA #$05 (2 cycles)
      0xAA,       // TAX      (2 cycles)
      0xE8        // INX      (2 cycles)
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Cycle(6);

    cpu.A.Should().Be(0x05);
    cpu.X.Should().Be(0x06);
    cpu.PC.Should().Be(0x8004);
  }

  [Fact]
  public void LoadRom_ThrowsException_WhenRomExceedsAddressSpace()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    byte[] oversizedRom = new byte[0x8001];

    var act = () => cpu.LoadRom(oversizedRom, 0x8000);

    act.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void Step_BranchTaken_ExecutesAndAdvancesPcToTarget()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA9, 0x80, // LDA #$80 (sets Negative flag)
      0x30, 0x04  // BMI +4 -> jumps to 0x8008
    ];
    cpu.LoadRom(program, 0x8000);

    var ldaCycles = cpu.Step();
    var branchCycles = cpu.Step();

    ldaCycles.Should().Be(2);
    branchCycles.Should().Be(3);
    cpu.PC.Should().Be(0x8008);
  }

  [Fact]
  public void Step_BranchNotTaken_AdvancesPcToNextInstruction()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA9, 0x01, // LDA #$01 (clears Negative flag)
      0x30, 0x04  // BMI +4 -> not taken
    ];
    cpu.LoadRom(program, 0x8000);

    var ldaCycles = cpu.Step();
    var branchCycles = cpu.Step();

    ldaCycles.Should().Be(2);
    branchCycles.Should().Be(2);
    cpu.PC.Should().Be(0x8004);
  }

  [Fact]
  public void Step_Dex_DecrementsXAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA2, 0x05, // LDX #$05
      0xCA        // DEX
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    cpu.X.Should().Be(0x04);
    cpu.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_Dey_DecrementsYAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA0, 0x05, // LDY #$05
      0x88        // DEY
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    cpu.Y.Should().Be(0x04);
    cpu.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_AslAccumulator_ShiftsLeftAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA9, 0x40, // LDA #$40
      0x0A        // ASL A
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    cpu.A.Should().Be(0x80);
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
    cpu.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_AslAbsoluteX_ModifiesMemoryAndReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2004, 0x03);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA2, 0x04,       // LDX #$04
      0x1E, 0x00, 0x20  // ASL $2000,X
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    bus.ReadByte(0x2004).Should().Be(0x06);
    cpu.PC.Should().Be(0x8005);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Step_LsrAccumulator_ShiftsRightAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA9, 0x03, // LDA #$03
      0x4A        // LSR A
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    cpu.A.Should().Be(0x01);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cpu.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_LsrAbsoluteX_ModifiesMemoryAndReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2004, 0x05);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA2, 0x04,       // LDX #$04
      0x5E, 0x00, 0x20  // LSR $2000,X
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    bus.ReadByte(0x2004).Should().Be(0x02);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.PC.Should().Be(0x8005);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Step_RolAccumulator_RotatesLeftAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA9, 0x40, // LDA #$40
      0x2A        // ROL A
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    cpu.A.Should().Be(0x80);
    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
    cpu.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_RolAbsoluteX_ModifiesMemoryAndReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2004, 0x03);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA2, 0x04,       // LDX #$04
      0x3E, 0x00, 0x20  // ROL $2000,X
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    bus.ReadByte(0x2004).Should().Be(0x06);
    cpu.PC.Should().Be(0x8005);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Step_RorAccumulator_RotatesRightAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA9, 0x03, // LDA #$03
      0x6A        // ROR A
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    cpu.A.Should().Be(0x01);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cpu.PC.Should().Be(0x8003);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Step_RorAbsoluteX_ModifiesMemoryAndReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2004, 0x05);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA2, 0x04,       // LDX #$04
      0x7E, 0x00, 0x20  // ROR $2000,X
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    bus.ReadByte(0x2004).Should().Be(0x02);
    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.PC.Should().Be(0x8005);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Step_IncZeroPage_ModifiesMemoryAndReturnsFiveCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x0042, 0x05);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xE6, 0x42 // INC $42
    ];
    cpu.LoadRom(program, 0x8000);

    var cycles = cpu.Step();

    bus.ReadByte(0x0042).Should().Be(0x06);
    cpu.PC.Should().Be(0x8002);
    cycles.Should().Be(5);
  }

  [Fact]
  public void Step_IncZeroPageX_ModifiesMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x0045, 0x10);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA2, 0x05, // LDX #$05
      0xF6, 0x40  // INC $40,X
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    bus.ReadByte(0x0045).Should().Be(0x11);
    cpu.PC.Should().Be(0x8004);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Step_IncAbsolute_ModifiesMemoryAndReturnsSixCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2000, 0x20);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xEE, 0x00, 0x20 // INC $2000
    ];
    cpu.LoadRom(program, 0x8000);

    var cycles = cpu.Step();

    bus.ReadByte(0x2000).Should().Be(0x21);
    cpu.PC.Should().Be(0x8003);
    cycles.Should().Be(6);
  }

  [Fact]
  public void Step_IncAbsoluteX_ModifiesMemoryAndReturnsSevenCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x2004, 0x09);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA2, 0x04,       // LDX #$04
      0xFE, 0x00, 0x20  // INC $2000,X
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    bus.ReadByte(0x2004).Should().Be(0x0A);
    cpu.PC.Should().Be(0x8005);
    cycles.Should().Be(7);
  }

  [Fact]
  public void Step_Pha_PushesAccumulatorAndAdvancesPcByOne()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA9, 0x33, // LDA #$33
      0x48        // PHA
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    var cycles = cpu.Step();

    bus.ReadByte(0x01FD).Should().Be(0x33);
    cpu.StackPointer.Should().Be(0xFC);
    cpu.PC.Should().Be(0x8003);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Step_Php_PushesStatusWithBreakAndUnusedBitsAndAdvancesPcByOne()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0x08 // PHP
    ];
    cpu.LoadRom(program, 0x8000);

    var cycles = cpu.Step();

    bus.ReadByte(0x01FD).Should().Be((u8)(Status.Interrupt | Status.Break | Status.Unused));
    cpu.StackPointer.Should().Be(0xFC);
    cpu.PC.Should().Be(0x8001);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Step_Pla_PullsAccumulatorAndAdvancesPcByOne()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x01FD, 0x77);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA2, 0xFC, // LDX #$FC
      0x9A,       // TXS
      0x68        // PLA
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    cpu.Step();
    var cycles = cpu.Step();

    cpu.A.Should().Be(0x77);
    cpu.StackPointer.Should().Be(0xFD);
    cpu.PC.Should().Be(0x8004);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Step_Plp_PullsStatusAndAdvancesPcByOne()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    bus.WriteByte(0x01FD, (u8)(Status.Carry | Status.Zero));
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0xA2, 0xFC, // LDX #$FC
      0x9A,       // TXS
      0x28        // PLP
    ];
    cpu.LoadRom(program, 0x8000);

    cpu.Step();
    cpu.Step();
    var cycles = cpu.Step();

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cpu.StackPointer.Should().Be(0xFD);
    cpu.PC.Should().Be(0x8004);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Step_JmpAbsolute_SetsProgramCounterAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0x4C, 0x50, 0x90 // JMP $9050
    ];
    cpu.LoadRom(program, 0x8000);

    var cycles = cpu.Step();

    cpu.PC.Should().Be(0x9050);
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
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0x6C, 0x00, 0x10 // JMP ($1000)
    ];
    cpu.LoadRom(program, 0x8000);

    var cycles = cpu.Step();

    cpu.PC.Should().Be(0x9050);
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
    var cpu = new Emulator(bus);
    byte[] program =
    [
      0x6C, 0xFF, 0x10 // JMP ($10FF)
    ];
    cpu.LoadRom(program, 0x8000);

    var cycles = cpu.Step();

    cpu.PC.Should().Be(0x9050);
    cycles.Should().Be(5);
  }
}
