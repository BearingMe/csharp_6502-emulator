using System.Numerics;

namespace mos6502.src.Domain.Objects;

public readonly record struct AddressingResult<T>(
  T Value,
  cycle Cycles
) where T : INumber<T>;
