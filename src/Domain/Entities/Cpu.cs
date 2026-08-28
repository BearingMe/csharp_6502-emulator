using Mos6502.Domain.Enums;

namespace Mos6502.Domain.Entities;

public class Cpu
{
  public u8 A { get; set; }
  public u8 X { get; set; }
  public u8 Y { get; set; }
  public u8 StackPointer { get; set; }
  public u16 PC { get; set; }
  public Status Status { get; set; }

  public Cpu()
  {
    Reset(0x0000);
  }

  public void Reset(u16 resetVectorAddress)
  {
    A = 0x00;
    X = 0x00;
    Y = 0x00;
    StackPointer = 0xFD;
    PC = resetVectorAddress;
    Status = Status.Interrupt;
  }
}
