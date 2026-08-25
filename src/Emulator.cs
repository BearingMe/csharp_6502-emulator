namespace mos6502;

public class Emulator
{
  private readonly Bus _bus;
  private readonly Addressing _addressing;
  private readonly Instructions _instructions;

  public u8 A { get; internal set; }
  public u8 X { get; internal set; }
  public u8 Y { get; internal set; }
  public u8 StackPointer { get; internal set; }
  public u16 PC { get; internal set; }
  public Status Status { get; internal set; }

  public Emulator(Bus bus)
  {
    _bus = bus;
    _addressing = new Addressing(bus, this);
    _instructions = new Instructions(bus, this);
    A = 0x00;
    X = 0x00;
    Y = 0x00;
    StackPointer = 0xFD;
    PC = _bus.ReadWord(0xFFFC);
    Status = 0x00 | Status.Interrupt;
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
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDA_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
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
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDX_zero_page_y(u8 operand)
  {
    var addressMode = _addressing.ZeroPageY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDX_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDX_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
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
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDY_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDY_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.LDY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle LDY_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
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
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ADC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ADC_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
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
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.SBC(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle SBC_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
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
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.AND(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle AND_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
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
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.ORA(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle ORA_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
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
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.EOR(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle EOR_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
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
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_zero_page_x(u8 operand)
  {
    var addressMode = _addressing.ZeroPageX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_absolute_x(u16 operand)
  {
    var addressMode = _addressing.AbsoluteX(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_absolute_y(u16 operand)
  {
    var addressMode = _addressing.AbsoluteY(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_indexed_indirect(u8 operand)
  {
    var addressMode = _addressing.IndexedIndirect(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CMP(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CMP_indirect_indexed(u8 operand)
  {
    var addressMode = _addressing.IndirectIndexed(operand);
    var value = _bus.ReadByte(addressMode.Value);
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
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CPX(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CPX_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
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
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CPY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle CPY_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.CPY(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BIT_zero_page(u8 operand)
  {
    var addressMode = _addressing.ZeroPage(operand);
    var value = _bus.ReadByte(addressMode.Value);
    var result = _instructions.BIT(value);

    return result.Cycles + addressMode.Cycles;
  }

  public cycle BIT_absolute(u16 operand)
  {
    var addressMode = _addressing.Absolute(operand);
    var value = _bus.ReadByte(addressMode.Value);
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
    PC = _bus.ReadWord(0xFFFC);
    Status = 0x00 | Status.Interrupt;
  }
}
