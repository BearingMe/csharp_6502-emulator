using System.Numerics;

namespace mos6502.src;

class Cpu
{
  private byte[] memory;
  private Status flags;
  private ushort pc;
  private ushort sp;
  private byte a;
  private byte x;
  private byte y;


  public Cpu()
  {
    memory = new byte[0x10000]; // 64 KiB
    flags = Status.Interrupt;
    pc = 0xFFFC;
    sp = 0x00FD;
    a = 0;
    x = 0;
    y = 0;
  }

  public void Reset()
  {
    flags = Status.Interrupt;
    pc = 0xFFFC;
    sp -= 3;
  }

  private void SetFlag(Status flag, bool active)
  {
    if (active)
    {
      // 0 | 0 = 0
      // 1 | 0 = 1
      // 0 | 1 = 1
      // 1 | 1 = 1
      flags |= flag;
    }
    else
    {
      // ~0 = 1;
      var invertedFlag = ~flag;

      // 0 & 0 = 0
      // 1 & 0 = 0
      // 0 & 1 = 0
      // 1 & 1 = 1
      flags &= invertedFlag;
    }
  }

  private bool IsFlag(Status flag)
  {
    return (flags & flag) > 0;
  }

  static byte ToByte<T>(T word) where T : INumber<T>
  {
    return byte.CreateTruncating(word);
  }

  static bool ToBool<T>(T word) where T : INumber<T>
  {
    return word > T.One;
  }


  public void Execute(Instruction instruction)
  {
    switch (instruction.Opcode)
    {
      // ADC (Add with Carry)
      case 0x69:
        int operand = (int)instruction.Operand!;
        int temp = a + operand + (IsFlag(Status.Carry) ? 1 : 0);

        SetFlag(Status.Carry, temp > 0xFF);
        SetFlag(Status.Zero, (temp & 0xFF) == 0);
        SetFlag(Status.Overflow, ToBool((temp ^ a) & (temp ^ operand) & 0b1000_0000));
        SetFlag(Status.Negative, ToBool(temp & 0b1000_0000));

        a = ToByte(temp);
        break;


      // AND (Bitwise AND)
      // ASL (Arithmetic Shift Left)
      // BCC (Branch if Carry Clear)
      // BCS (Branch if Carry Set)
      // BEQ (Branch if Equal)
      // BIT (Bit Test)
      // BMI (Branch if Minus)
      // BNE (Branch if Not Equal)
      // BPL (Branch if Plus)
      // BRK (Break (software IRQ))
      // BVC (Branch if Overflow Clear)
      // BVS (Branch if Overflow Set)
      // CLC (Clear Carry)
      // CLD (Clear Decimal)
      // CLI (Clear Interrupt Disable)
      // CLV (Clear Overflow)
      // CMP (Compare A)
      // CPX (Compare X)
      // CPY (Compare Y)
      // DEC (Decrement Memory)
      // DEX (Decrement X)
      // DEY (Decrement Y)
      // EOR (Bitwise Exclusive OR)
      // INC (Increment Memory)
      // INX (Increment X)
      // INY (Increment Y)
      // JMP (Jump)
      // JSR (Jump to Subroutine)
      // LDA (Load A)
      // LDX (Load X)
      // LDY (Load Y)
      // LSR (Logical Shift Right)
      // NOP (No Operation)
      // ORA (Bitwise OR)
      // PHA (Push A)
      // PHP (Push Processor Status)
      // PLA (Pull A)
      // PLP (Pull Processor Status)
      // ROL (Rotate Left)
      // ROR (Rotate Right)
      // RTI (Return from Interrupt)
      // RTS (Return from Subroutine)
      // SBC (Subtract with Carry)
      // SEC (Set Carry)
      // SED (Set Decimal)
      // SEI (Set Interrupt Disable)
      // STA (Store A)
      // STX (Store X)
      // STY (Store Y)
      // TAX (Transfer A to X)
      // TAY (Transfer A to Y)
      // TSX (Transfer Stack Pointer to X)
      // TXA (Transfer X to A)
      // TXS (Transfer X to Stack Pointer)
      // TYA (Transfer Y to A)
      default:
        throw new Exception($"Unknown opcode: {instruction:X2}");
    }
  }
}