namespace mos6502.src.Domain.Objects;

using U8 = Unassigned8Bits;
using U16 = Unassigned16Bits;

public readonly record struct Unassigned16Bits(ushort Value)
{
  // operators
  public static int operator +(U16 a, U16 b) => a.Value + b.Value;
  public static int operator -(U16 a, U16 b) => a.Value - b.Value;

  public static int operator &(U16 a, U16 b) => a.Value & b.Value;
  public static int operator |(U16 a, U16 b) => a.Value | b.Value;
  public static int operator ^(U16 a, U16 b) => a.Value ^ b.Value;

  public static int operator <<(U16 a, int shift) => a.Value << shift;
  public static int operator >>(U16 a, int shift) => a.Value >> shift;

  // casting
  public static implicit operator int(U16 value) => value.Value;
  public static implicit operator U16(ushort value) => new(value);
  public static implicit operator U16(U8 value) => new(value.Value);
  public static explicit operator U16(int value) => new((ushort)value);
}

