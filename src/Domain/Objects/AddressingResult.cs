using System.Numerics;

namespace Mos6502.Domain.Objects;

public readonly record struct AddressingResult<T>(
  T Value,
  cycle Cycles
) where T : INumber<T>;
