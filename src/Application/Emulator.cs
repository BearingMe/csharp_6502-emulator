using Mos6502.Domain.Entities;
using Mos6502.Domain.Enums;

namespace Mos6502.Application;

public class Emulator
{
  private readonly Bus _bus;
  private readonly Cpu _cpu;
  private readonly Addressing _addressing;
  private readonly Instructions _instructions;

  public Bus Bus => _bus;
  public Cpu Cpu => _cpu;

  public u8 A { get => _cpu.A; set => _cpu.A = value; }
  public u8 X { get => _cpu.X; set => _cpu.X = value; }
  public u8 Y { get => _cpu.Y; set => _cpu.Y = value; }
  public u8 StackPointer { get => _cpu.StackPointer; set => _cpu.StackPointer = value; }
  public u16 PC { get => _cpu.PC; set => _cpu.PC = value; }
  public Status Status { get => _cpu.Status; set => _cpu.Status = value; }

  public Emulator(Bus bus)
  {
    _bus = bus;
    _cpu = new Cpu();
    _addressing = new Addressing(_cpu, _bus);
    _instructions = new Instructions(_cpu, _bus);

    _cpu.Reset(_bus.ReadWord(0xFFFC));
  }

  public cycle Step()
  {
    var opcode = FetchByte();

    return opcode switch
    {
      // --- ADC ---
      0x69 => AdcImmediate(FetchByte()),
      0x65 => AdcZeroPage(FetchByte()),
      0x75 => AdcZeroPageX(FetchByte()),
      0x6D => AdcAbsolute(FetchWord()),
      0x7D => AdcAbsoluteX(FetchWord()),
      0x79 => AdcAbsoluteY(FetchWord()),
      0x61 => AdcIndexedIndirect(FetchByte()),
      0x71 => AdcIndirectIndexed(FetchByte()),

      // --- AND ---
      0x29 => AndImmediate(FetchByte()),
      0x25 => AndZeroPage(FetchByte()),
      0x35 => AndZeroPageX(FetchByte()),
      0x2D => AndAbsolute(FetchWord()),
      0x3D => AndAbsoluteX(FetchWord()),
      0x39 => AndAbsoluteY(FetchWord()),
      0x21 => AndIndexedIndirect(FetchByte()),
      0x31 => AndIndirectIndexed(FetchByte()),

      // --- ASL ---
      0x0A => AslAccumulator(),
      0x06 => AslZeroPage(FetchByte()),
      0x16 => AslZeroPageX(FetchByte()),
      0x0E => AslAbsolute(FetchWord()),
      0x1E => AslAbsoluteX(FetchWord()),

      // --- Branches ---
      0x90 => BccRelative(FetchByte()),
      0xB0 => BcsRelative(FetchByte()),
      0xF0 => BeqRelative(FetchByte()),
      0x30 => BmiRelative(FetchByte()),
      0xD0 => BneRelative(FetchByte()),
      0x10 => BplRelative(FetchByte()),
      0x50 => BvcRelative(FetchByte()),
      0x70 => BvsRelative(FetchByte()),

      // --- BIT ---
      0x24 => BitZeroPage(FetchByte()),
      0x2C => BitAbsolute(FetchWord()),

      // --- System / Return ---
      0x00 => Brk(),
      0x18 => Clc(),
      0xD8 => Cld(),
      0x58 => Cli(),
      0xB8 => Clv(),

      // --- CMP / CPX / CPY ---
      0xC9 => CmpImmediate(FetchByte()),
      0xC5 => CmpZeroPage(FetchByte()),
      0xD5 => CmpZeroPageX(FetchByte()),
      0xCD => CmpAbsolute(FetchWord()),
      0xDD => CmpAbsoluteX(FetchWord()),
      0xD9 => CmpAbsoluteY(FetchWord()),
      0xC1 => CmpIndexedIndirect(FetchByte()),
      0xD1 => CmpIndirectIndexed(FetchByte()),

      0xE0 => CpxImmediate(FetchByte()),
      0xE4 => CpxZeroPage(FetchByte()),
      0xEC => CpxAbsolute(FetchWord()),

      0xC0 => CpyImmediate(FetchByte()),
      0xC4 => CpyZeroPage(FetchByte()),
      0xCC => CpyAbsolute(FetchWord()),

      // --- DEC / DEX / DEY ---
      0xC6 => DecZeroPage(FetchByte()),
      0xD6 => DecZeroPageX(FetchByte()),
      0xCE => DecAbsolute(FetchWord()),
      0xDE => DecAbsoluteX(FetchWord()),
      0xCA => Dex(),
      0x88 => Dey(),

      // --- EOR ---
      0x49 => EorImmediate(FetchByte()),
      0x45 => EorZeroPage(FetchByte()),
      0x55 => EorZeroPageX(FetchByte()),
      0x4D => EorAbsolute(FetchWord()),
      0x5D => EorAbsoluteX(FetchWord()),
      0x59 => EorAbsoluteY(FetchWord()),
      0x41 => EorIndexedIndirect(FetchByte()),
      0x51 => EorIndirectIndexed(FetchByte()),

      // --- INC / INX / INY ---
      0xE6 => IncZeroPage(FetchByte()),
      0xF6 => IncZeroPageX(FetchByte()),
      0xEE => IncAbsolute(FetchWord()),
      0xFE => IncAbsoluteX(FetchWord()),
      0xE8 => Inx(),
      0xC8 => Iny(),

      // --- JMP / JSR ---
      0x4C => JmpAbsolute(FetchWord()),
      0x6C => JmpIndirect(FetchWord()),
      0x20 => JsrAbsolute(FetchWord()),

      // --- LDA ---
      0xA9 => LdaImmediate(FetchByte()),
      0xA5 => LdaZeroPage(FetchByte()),
      0xB5 => LdaZeroPageX(FetchByte()),
      0xAD => LdaAbsolute(FetchWord()),
      0xBD => LdaAbsoluteX(FetchWord()),
      0xB9 => LdaAbsoluteY(FetchWord()),
      0xA1 => LdaIndexedIndirect(FetchByte()),
      0xB1 => LdaIndirectIndexed(FetchByte()),

      // --- LDX ---
      0xA2 => LdxImmediate(FetchByte()),
      0xA6 => LdxZeroPage(FetchByte()),
      0xB6 => LdxZeroPageY(FetchByte()),
      0xAE => LdxAbsolute(FetchWord()),
      0xBE => LdxAbsoluteY(FetchWord()),

      // --- LDY ---
      0xA0 => LdyImmediate(FetchByte()),
      0xA4 => LdyZeroPage(FetchByte()),
      0xB4 => LdyZeroPageX(FetchByte()),
      0xAC => LdyAbsolute(FetchWord()),
      0xBC => LdyAbsoluteX(FetchWord()),

      // --- LSR ---
      0x4A => LsrAccumulator(),
      0x46 => LsrZeroPage(FetchByte()),
      0x56 => LsrZeroPageX(FetchByte()),
      0x4E => LsrAbsolute(FetchWord()),
      0x5E => LsrAbsoluteX(FetchWord()),

      // --- NOP ---
      0xEA => Nop(),

      // --- ORA ---
      0x09 => OraImmediate(FetchByte()),
      0x05 => OraZeroPage(FetchByte()),
      0x15 => OraZeroPageX(FetchByte()),
      0x0D => OraAbsolute(FetchWord()),
      0x1D => OraAbsoluteX(FetchWord()),
      0x19 => OraAbsoluteY(FetchWord()),
      0x01 => OraIndexedIndirect(FetchByte()),
      0x11 => OraIndirectIndexed(FetchByte()),

      // --- Stack ---
      0x48 => Pha(),
      0x08 => Php(),
      0x68 => Pla(),
      0x28 => Plp(),

      // --- ROL ---
      0x2A => RolAccumulator(),
      0x26 => RolZeroPage(FetchByte()),
      0x36 => RolZeroPageX(FetchByte()),
      0x2E => RolAbsolute(FetchWord()),
      0x3E => RolAbsoluteX(FetchWord()),

      // --- ROR ---
      0x6A => RorAccumulator(),
      0x66 => RorZeroPage(FetchByte()),
      0x76 => RorZeroPageX(FetchByte()),
      0x6E => RorAbsolute(FetchWord()),
      0x7E => RorAbsoluteX(FetchWord()),

      // --- Return ---
      0x40 => Rti(),
      0x60 => Rts(),

      // --- SBC ---
      0xE9 => SbcImmediate(FetchByte()),
      0xE5 => SbcZeroPage(FetchByte()),
      0xF5 => SbcZeroPageX(FetchByte()),
      0xED => SbcAbsolute(FetchWord()),
      0xFD => SbcAbsoluteX(FetchWord()),
      0xF9 => SbcAbsoluteY(FetchWord()),
      0xE1 => SbcIndexedIndirect(FetchByte()),
      0xF1 => SbcIndirectIndexed(FetchByte()),

      // --- Flag sets ---
      0x38 => Sec(),
      0xF8 => Sed(),
      0x78 => Sei(),

      // --- STA ---
      0x85 => StaZeroPage(FetchByte()),
      0x95 => StaZeroPageX(FetchByte()),
      0x8D => StaAbsolute(FetchWord()),
      0x9D => StaAbsoluteX(FetchWord()),
      0x99 => StaAbsoluteY(FetchWord()),
      0x81 => StaIndexedIndirect(FetchByte()),
      0x91 => StaIndirectIndexed(FetchByte()),

      // --- STX ---
      0x86 => StxZeroPage(FetchByte()),
      0x96 => StxZeroPageY(FetchByte()),
      0x8E => StxAbsolute(FetchWord()),

      // --- STY ---
      0x84 => StyZeroPage(FetchByte()),
      0x94 => StyZeroPageX(FetchByte()),
      0x8C => StyAbsolute(FetchWord()),

      // --- Transfers ---
      0xAA => Tax(),
      0xA8 => Tay(),
      0xBA => Tsx(),
      0x8A => Txa(),
      0x9A => Txs(),
      0x98 => Tya(),

      _ => throw new InvalidOperationException($"Unknown or unimplemented opcode: 0x{opcode:X2}")
    };
  }

  public cycle AdcAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AdcAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AdcAbsoluteY(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AdcImmediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.ADC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AdcIndexedIndirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AdcIndirectIndexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AdcZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AdcZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AndAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AndAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AndAbsoluteY(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AndImmediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.AND(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AndIndexedIndirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AndIndirectIndexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AndZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AndZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AslAccumulator()
  {
    var result = _instructions.ASL();

    return result.Cycles;
  }

  public cycle AslAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.ASL(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AslAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var result = _instructions.ASL(addressMode.Value);

    return 7;
  }

  public cycle AslZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.ASL(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AslZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var result = _instructions.ASL(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BccRelative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BCC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BcsRelative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BCS(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BeqRelative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BEQ(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BitAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.BIT(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BitZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.BIT(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BmiRelative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BMI(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BneRelative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BNE(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BplRelative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BPL(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle Brk()
  {
    var result = _instructions.BRK();

    return result.Cycles;
  }

  public cycle BvcRelative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BVC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BvsRelative(u8 operand)
  {
    var addressMode = _addressing.Relative(operand);
    var result = _instructions.BVS(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle Clc()
  {
    var result = _instructions.CLC();

    return result.Cycles;
  }

  public cycle Cld()
  {
    var result = _instructions.CLD();

    return result.Cycles;
  }

  public cycle Cli()
  {
    var result = _instructions.CLI();

    return result.Cycles;
  }

  public cycle Clv()
  {
    var result = _instructions.CLV();

    return result.Cycles;
  }

  public cycle CmpAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CmpAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CmpAbsoluteY(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CmpImmediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.CMP(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CmpIndexedIndirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CmpIndirectIndexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CmpZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CmpZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CpxAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CPX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CpxImmediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.CPX(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CpxZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CPX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CpyAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CPY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CpyImmediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.CPY(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CpyZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CPY(value);

    return result.Cycles + addressMode.Cycles;
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

  public cycle DecAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.DEC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle DecAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var result = _instructions.DEC(addressMode.Value);

    return result.Cycles + 3;
  }

  public cycle DecZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.DEC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle DecZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var result = _instructions.DEC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle Dex()
  {
    var result = _instructions.DEX();

    return result.Cycles;
  }

  public cycle Dey()
  {
    var result = _instructions.DEY();

    return result.Cycles;
  }

  public cycle EorAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EorAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EorAbsoluteY(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EorImmediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.EOR(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EorIndexedIndirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EorIndirectIndexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EorZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EorZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle IncAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.INC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle IncAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var result = _instructions.INC(addressMode.Value);

    return result.Cycles + 3;
  }

  public cycle IncZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.INC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle IncZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var result = _instructions.INC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle Inx()
  {
    var result = _instructions.INX();

    return result.Cycles;
  }

  public cycle Iny()
  {
    var result = _instructions.INY();

    return result.Cycles;
  }

  public cycle JmpAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.JMP(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle JmpIndirect(u16 operand)
  {
    var addressMode = _addressing.Indirect(operand);
    var result = _instructions.JMP(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle JsrAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.JSR(addressMode.Value);

    return result.Cycles;
  }

  public cycle LdaAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdaAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdaAbsoluteY(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdaImmediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.LDA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdaIndexedIndirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdaIndirectIndexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdaZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdaZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdxAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdxAbsoluteY(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdxImmediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.LDX(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdxZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdxZeroPageY(u8 operand)
  {
    var addressMode = _addressing.ZeroPageY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdyAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdyAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdyImmediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.LDY(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdyZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LdyZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public void LoadRom(byte[] rom, u16 startAddress = 0x8000)
  {
    if ((long)startAddress + rom.Length > 0x10000)
    {
      throw new ArgumentOutOfRangeException(nameof(rom), "ROM exceeds addressable 64KB memory space.");
    }

    for (var i = 0; i < rom.Length; i++)
    {
      _bus.WriteByte((u16)(startAddress + i), rom[i]);
    }
  }

  public cycle LsrAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.LSR(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LsrAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var result = _instructions.LSR(addressMode.Value);

    return 7;
  }

  public cycle LsrAccumulator()
  {
    var result = _instructions.LSR();

    return result.Cycles;
  }

  public cycle LsrZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.LSR(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LsrZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var result = _instructions.LSR(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle Nop()
  {
    var result = _instructions.NOP();

    return result.Cycles;
  }

  public cycle OraAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle OraAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle OraAbsoluteY(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle OraImmediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.ORA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle OraIndexedIndirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle OraIndirectIndexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle OraZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle OraZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle Pha()
  {
    var result = _instructions.PHA();

    return result.Cycles;
  }

  public cycle Php()
  {
    var result = _instructions.PHP();

    return result.Cycles;
  }

  public cycle Pla()
  {
    var result = _instructions.PLA();

    return result.Cycles;
  }

  public cycle Plp()
  {
    var result = _instructions.PLP();

    return result.Cycles;
  }

  public void Reset()
  {
    _cpu.StackPointer = (u8)(_cpu.StackPointer - 3);
    _cpu.PC = _bus.ReadWord(0xFFFC);
    _cpu.Status = Status.Interrupt;
  }

  public cycle RolAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.ROL(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle RolAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var result = _instructions.ROL(addressMode.Value);

    return result.Cycles + 3;
  }

  public cycle RolAccumulator()
  {
    var result = _instructions.ROL();

    return result.Cycles;
  }

  public cycle RolZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.ROL(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle RolZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var result = _instructions.ROL(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle RorAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.ROR(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle RorAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var result = _instructions.ROR(addressMode.Value);

    return result.Cycles + 3;
  }

  public cycle RorAccumulator()
  {
    var result = _instructions.ROR();

    return result.Cycles;
  }

  public cycle RorZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.ROR(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle RorZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var result = _instructions.ROR(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle Rti()
  {
    var result = _instructions.RTI();

    return result.Cycles;
  }

  public cycle Rts()
  {
    var result = _instructions.RTS();

    return result.Cycles;
  }

  public cycle SbcAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SbcAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SbcAbsoluteY(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SbcImmediate(u8 operand)
  {
    var addressMode = _addressing.Immediate(operand);
    var result = _instructions.SBC(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SbcIndexedIndirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SbcIndirectIndexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SbcZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SbcZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle Sec()
  {
    var result = _instructions.SEC();

    return result.Cycles;
  }

  public cycle Sed()
  {
    var result = _instructions.SED();

    return result.Cycles;
  }

  public cycle Sei()
  {
    var result = _instructions.SEI();

    return result.Cycles;
  }

  public cycle StaAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.STA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle StaAbsoluteX(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var result = _instructions.STA(addressMode.Value);

    return 5;
  }

  public cycle StaAbsoluteY(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var result = _instructions.STA(addressMode.Value);

    return 5;
  }

  public cycle StaIndexedIndirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var result = _instructions.STA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle StaIndirectIndexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var result = _instructions.STA(addressMode.Value);

    return 6;
  }

  public cycle StaZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.STA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle StaZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var result = _instructions.STA(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle StxAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.STX(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle StxZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.STX(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle StxZeroPageY(u8 operand)
  {
    var addressMode = _addressing.ZeroPageY(operand);
    var result = _instructions.STX(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle StyAbsolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var result = _instructions.STY(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle StyZeroPage(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var result = _instructions.STY(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle StyZeroPageX(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var result = _instructions.STY(addressMode.Value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle Tax()
  {
    var result = _instructions.TAX();

    return result.Cycles;
  }

  public cycle Tay()
  {
    var result = _instructions.TAY();

    return result.Cycles;
  }

  public cycle Tsx()
  {
    var result = _instructions.TSX();

    return result.Cycles;
  }

  public cycle Txa()
  {
    var result = _instructions.TXA();

    return result.Cycles;
  }

  public cycle Txs()
  {
    var result = _instructions.TXS();

    return result.Cycles;
  }

  public cycle Tya()
  {
    var result = _instructions.TYA();

    return result.Cycles;
  }

  private u8 FetchByte()
  {
    var value = _bus.ReadByte(_cpu.PC);
    _cpu.PC++;
    return value;
  }

  private u16 FetchWord()
  {
    var value = _bus.ReadWord(_cpu.PC);
    _cpu.PC += 2;
    return value;
  }
}
