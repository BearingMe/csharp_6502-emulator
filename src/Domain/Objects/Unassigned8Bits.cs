namespace mos6502.src.Domain.Objects;

using U8 = Unassigned8Bits;
using U16 = Unassigned16Bits;

public readonly record struct Unassigned8Bits(byte Value)
{
  // operators
  public static int operator +(U8 a, U8 b) => a.Value + b.Value;
  public static int operator -(U8 a, U8 b) => a.Value - b.Value;

  public static int operator &(U8 a, U8 b) => a.Value & b.Value;
  public static int operator |(U8 a, U8 b) => a.Value | b.Value;
  public static int operator ^(U8 a, U8 b) => a.Value ^ b.Value;

  public static int operator <<(U8 a, int shift) => a.Value << shift;
  public static int operator >>(U8 a, int shift) => a.Value >> shift;

  // casting
  public static implicit operator int(U8 value) => value.Value;
  public static implicit operator U8(byte value) => new(value);
  public static explicit operator U8(int value) => new((byte)value);
  public static explicit operator U8(U16 value) => new((byte)value.Value);
}