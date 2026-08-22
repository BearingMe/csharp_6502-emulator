using mos6502.src.Domain.Enums;

namespace mos6502.src.Domain.Entities;

public record struct Registers(
   u16 PC,
   u16 Stkp,
   u8 Acc,
   u8 X,
   u8 Y,
   Status Status
)
{
  public Registers() : this(0xFFFC, 0x00FD, 0x00, 0x00, 0x00, Status.Interrupt) { }

  public void SetFlag(Status flag, bool active)
  {
    if (active) Status |= flag;
    else Status &= ~flag;
  }

  public readonly bool IsFlag(Status flag)
  {
    return (Status & flag) > 0;
  }
}
