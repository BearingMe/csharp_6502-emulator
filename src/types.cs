namespace mos6502.src;

public readonly record struct Instruction(
    byte Opcode,
    ushort? Operand
);

