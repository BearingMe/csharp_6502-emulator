namespace mos6502;

[Flags]
public enum Status : u8
{
  Carry = 1 << 0,
  Zero = 1 << 1,
  Interrupt = 1 << 2,
  Decimal = 1 << 3,
  Break = 1 << 4,
  Unused = 1 << 5,
  Overflow = 1 << 6,
  Negative = 1 << 7
}

public enum AddressingMode
{
  Implied,
  Accumulator,
  Immediate,
  ZeroPage,
  ZeroPageX,
  ZeroPageY,
  Relative,
  Absolute,
  AbsoluteX,
  AbsoluteY,
  Indirect,
  IndexedIndirect,
  IndirectIndexed
}