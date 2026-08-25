using mos6502.src.Domain.Entities;
using mos6502.src.Domain.Enums;

namespace mos6502;

public class Bus : Cpu
{
}

public class Emulator
{
  private readonly Cpu _cpu;
  private readonly Addressing _addressing;
  private readonly Instructions _instructions;

  public u8 A { get => _cpu.A; internal set => _cpu.A = value; }
  public u8 X { get => _cpu.X; internal set => _cpu.X = value; }
  public u8 Y { get => _cpu.Y; internal set => _cpu.Y = value; }
  public u8 StackPointer { get => _cpu.StackPointer; internal set => _cpu.StackPointer = value; }
  public u16 PC { get => _cpu.PC; internal set => _cpu.PC = value; }
  public Status Status { get => _cpu.Status; internal set => _cpu.Status = value; }

  public Emulator(Cpu cpu)
  {
    _cpu = cpu;
    _addressing = cpu.Addressing;
    _instructions = cpu.Instructions;
    A = 0x00;
    X = 0x00;
    Y = 0x00;
    StackPointer = 0xFD;
    PC = _cpu.ReadWord(0xFFFC);
    Status = 0x00 | Status.Interrupt;
  }

  public cycle Step()
  {
    var opcode = _cpu.FetchByte();

    return opcode switch
    {
      // --- LDA ---
      0xA9 => LDA_immediate(_cpu.FetchByte()),
      0xA5 => LDA_zero_page(_cpu.FetchByte()),
      0xB5 => LDA_zero_page_x(_cpu.FetchByte()),
      0xAD => LDA_absolute(_cpu.FetchWord()),
      0xBD => LDA_absolute_x(_cpu.FetchWord()),
      0xB9 => LDA_absolute_y(_cpu.FetchWord()),
      0xA1 => LDA_indexed_indirect(_cpu.FetchByte()),
      0xB1 => LDA_indirect_indexed(_cpu.FetchByte()),

      // --- LDX ---
      0xA2 => LDX_immediate(_cpu.FetchByte()),
      0xA6 => LDX_zero_page(_cpu.FetchByte()),
      0xB6 => LDX_zero_page_y(_cpu.FetchByte()),
      0xAE => LDX_absolute(_cpu.FetchWord()),
      0xBE => LDX_absolute_y(_cpu.FetchWord()),

      // --- LDY ---
      0xA0 => LDY_immediate(_cpu.FetchByte()),
      0xA4 => LDY_zero_page(_cpu.FetchByte()),
      0xB4 => LDY_zero_page_x(_cpu.FetchByte()),
      0xAC => LDY_absolute(_cpu.FetchWord()),
      0xBC => LDY_absolute_x(_cpu.FetchWord()),

      // --- STA ---
      0x85 => STA_zero_page(_cpu.FetchByte()),
      0x95 => STA_zero_page_x(_cpu.FetchByte()),
      0x8D => STA_absolute(_cpu.FetchWord()),
      0x9D => STA_absolute_x(_cpu.FetchWord()),
      0x99 => STA_absolute_y(_cpu.FetchWord()),
      0x81 => STA_indexed_indirect(_cpu.FetchByte()),
      0x91 => STA_indirect_indexed(_cpu.FetchByte()),

      // --- STX ---
      0x86 => STX_zero_page(_cpu.FetchByte()),
      0x96 => STX_zero_page_y(_cpu.FetchByte()),
      0x8E => STX_absolute(_cpu.FetchWord()),

      // --- STY ---
      0x84 => STY_zero_page(_cpu.FetchByte()),
      0x94 => STY_zero_page_x(_cpu.FetchByte()),
      0x8C => STY_absolute(_cpu.FetchWord()),

      // --- Transfers ---
      0xAA => TAX(),
      0xA8 => TAY(),
      0xBA => TSX(),
      0x8A => TXA(),
      0x9A => TXS(),
      0x98 => TYA(),

      // --- Increments ---
      0xE8 => INX(),
      0xC8 => INY(),

      // --- ADC ---
      0x69 => ADC_immediate(_cpu.FetchByte()),
      0x65 => ADC_zero_page(_cpu.FetchByte()),
      0x75 => ADC_zero_page_x(_cpu.FetchByte()),
      0x6D => ADC_absolute(_cpu.FetchWord()),
      0x7D => ADC_absolute_x(_cpu.FetchWord()),
      0x79 => ADC_absolute_y(_cpu.FetchWord()),
      0x61 => ADC_indexed_indirect(_cpu.FetchByte()),
      0x71 => ADC_indirect_indexed(_cpu.FetchByte()),

      // --- SBC ---
      0xE9 => SBC_immediate(_cpu.FetchByte()),
      0xE5 => SBC_zero_page(_cpu.FetchByte()),
      0xF5 => SBC_zero_page_x(_cpu.FetchByte()),
      0xED => SBC_absolute(_cpu.FetchWord()),
      0xFD => SBC_absolute_x(_cpu.FetchWord()),
      0xF9 => SBC_absolute_y(_cpu.FetchWord()),
      0xE1 => SBC_indexed_indirect(_cpu.FetchByte()),
      0xF1 => SBC_indirect_indexed(_cpu.FetchByte()),

      // --- AND ---
      0x29 => AND_immediate(_cpu.FetchByte()),
      0x25 => AND_zero_page(_cpu.FetchByte()),
      0x35 => AND_zero_page_x(_cpu.FetchByte()),
      0x2D => AND_absolute(_cpu.FetchWord()),
      0x3D => AND_absolute_x(_cpu.FetchWord()),
      0x39 => AND_absolute_y(_cpu.FetchWord()),
      0x21 => AND_indexed_indirect(_cpu.FetchByte()),
      0x31 => AND_indirect_indexed(_cpu.FetchByte()),

      // --- ORA ---
      0x09 => ORA_immediate(_cpu.FetchByte()),
      0x05 => ORA_zero_page(_cpu.FetchByte()),
      0x15 => ORA_zero_page_x(_cpu.FetchByte()),
      0x0D => ORA_absolute(_cpu.FetchWord()),
      0x1D => ORA_absolute_x(_cpu.FetchWord()),
      0x19 => ORA_absolute_y(_cpu.FetchWord()),
      0x01 => ORA_indexed_indirect(_cpu.FetchByte()),
      0x11 => ORA_indirect_indexed(_cpu.FetchByte()),

      // --- EOR ---
      0x49 => EOR_immediate(_cpu.FetchByte()),
      0x45 => EOR_zero_page(_cpu.FetchByte()),
      0x55 => EOR_zero_page_x(_cpu.FetchByte()),
      0x4D => EOR_absolute(_cpu.FetchWord()),
      0x5D => EOR_absolute_x(_cpu.FetchWord()),
      0x59 => EOR_absolute_y(_cpu.FetchWord()),
      0x41 => EOR_indexed_indirect(_cpu.FetchByte()),
      0x51 => EOR_indirect_indexed(_cpu.FetchByte()),

      // --- BIT ---
      0x24 => BIT_zero_page(_cpu.FetchByte()),
      0x2C => BIT_absolute(_cpu.FetchWord()),

      // --- CMP ---
      0xC9 => CMP_immediate(_cpu.FetchByte()),
      0xC5 => CMP_zero_page(_cpu.FetchByte()),
      0xD5 => CMP_zero_page_x(_cpu.FetchByte()),
      0xCD => CMP_absolute(_cpu.FetchWord()),
      0xDD => CMP_absolute_x(_cpu.FetchWord()),
      0xD9 => CMP_absolute_y(_cpu.FetchWord()),
      0xC1 => CMP_indexed_indirect(_cpu.FetchByte()),
      0xD1 => CMP_indirect_indexed(_cpu.FetchByte()),

      // --- CPX ---
      0xE0 => CPX_immediate(_cpu.FetchByte()),
      0xE4 => CPX_zero_page(_cpu.FetchByte()),
      0xEC => CPX_absolute(_cpu.FetchWord()),

      // --- CPY ---
      0xC0 => CPY_immediate(_cpu.FetchByte()),
      0xC4 => CPY_zero_page(_cpu.FetchByte()),
      0xCC => CPY_absolute(_cpu.FetchWord()),

      // --- Branches ---
      0x90 => BCC_relative(_cpu.FetchByte()),
      0xB0 => BCS_relative(_cpu.FetchByte()),
      0xF0 => BEQ_relative(_cpu.FetchByte()),
      0xD0 => BNE_relative(_cpu.FetchByte()),
      0x30 => BMI_relative(_cpu.FetchByte()),
      0x10 => BPL_relative(_cpu.FetchByte()),
      0x50 => BVC_relative(_cpu.FetchByte()),
      0x70 => BVS_relative(_cpu.FetchByte()),

      _ => throw new InvalidOperationException($"Unknown or unimplemented opcode: 0x{opcode:X2}")
    };
  }

  public void LoadRom(byte[] rom, u16 startAddress = 0x8000)
  {
    if ((long)startAddress + rom.Length > 0x10000)
    {
      throw new ArgumentOutOfRangeException(nameof(rom), "ROM exceeds addressable 64KB memory space.");
    }

    for (var i = 0; i < rom.Length; i++)
    {
      _cpu.WriteByte((u16)(startAddress + i), rom[i]);
    }
  }

  public void Cycle(int? clock = 0)
  {
    var target = clock ?? 0;
    var cyclesRun = 0;

    while (cyclesRun < target)
    {
      cyclesRun += Step();
    }
  }

  public cycle LDA_immediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.LDA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDX_immediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.LDX(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDX_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDX_zero_page_y(u8 operand)
  {
    var addressMode = _addressing.ZeroPageY(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDX_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDX_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDY_immediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.LDY(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDY_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDY_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDY_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDY_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.LDY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STA_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.STA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STA_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var result = _instructions.STA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STA_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.STA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STA_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var result = _instructions.STA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STA_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var result = _instructions.STA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STA_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var result = _instructions.STA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STA_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var result = _instructions.STA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STX_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.STX(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STX_zero_page_y(u8 operand)
  {
    var addressMode = _addressing.ZeroPageY(operand);
    var result = _instructions.STX(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STX_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.STX(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STY_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.STY(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STY_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var result = _instructions.STY(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle STY_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.STY(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle TAX()
  {
    var result = _instructions.TAX();

    return result.Cycles;
  }

  public cycle TAY()
  {
    var result = _instructions.TAY();

    return result.Cycles;
  }

  public cycle TSX()
  {
    var result = _instructions.TSX();

    return result.Cycles;
  }

  public cycle TXA()
  {
    var result = _instructions.TXA();

    return result.Cycles;
  }

  public cycle TXS()
  {
    var result = _instructions.TXS();

    return result.Cycles;
  }

  public cycle TYA()
  {
    var result = _instructions.TYA();

    return result.Cycles;
  }

  public cycle INX()
  {
    var result = _instructions.INX();

    return result.Cycles;
  }

  public cycle INY()
  {
    var result = _instructions.INY();

    return result.Cycles;
  }

  public cycle ADC_immediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.ADC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_immediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.SBC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_immediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.AND(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_immediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.ORA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_immediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.EOR(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_immediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.CMP(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CPX_immediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.CPX(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CPX_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.CPX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CPX_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.CPX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CPY_immediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.CPY(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CPY_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.CPY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CPY_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.CPY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BIT_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.BIT(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BIT_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _cpu.ReadByte(addressMode.Value);
    var result = _instructions.BIT(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BCC_relative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BCC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BCS_relative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BCS(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BEQ_relative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BEQ(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BNE_relative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BNE(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BMI_relative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BMI(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BPL_relative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BPL(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BVC_relative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BVC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BVS_relative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BVS(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public void Reset()
  {
    StackPointer = (u8)(StackPointer - 3);
    PC = _cpu.ReadWord(0xFFFC);
    Status = 0x00 | Status.Interrupt;
  }
}
