using System.Numerics;

namespace mos6502;

public readonly record struct AddressingResult<T>(
  T Value,
  cycle Cycles
) where T : INumber<T>;

public readonly record struct InstructionResult(cycle Cycles = 0);
