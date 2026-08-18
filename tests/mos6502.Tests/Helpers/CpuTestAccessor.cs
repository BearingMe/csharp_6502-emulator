using mos6502.src.Domain.Entities;
using mos6502.src.Domain.Enums;
using mos6502.src.Domain.Objects;

namespace mos6502.Tests.Helpers;

internal static class CpuTestAccessor
{
    public static byte GetA(this Cpu cpu) => cpu.Accumulator.Value;
    public static void SetA(this Cpu cpu, byte value) => cpu.Accumulator = new Unassigned8Bits(value);

    public static byte GetX(this Cpu cpu) => cpu.XRegister.Value;
    public static void SetX(this Cpu cpu, byte value) => cpu.XRegister = new Unassigned8Bits(value);

    public static byte GetY(this Cpu cpu) => cpu.YRegister.Value;
    public static void SetY(this Cpu cpu, byte value) => cpu.YRegister = new Unassigned8Bits(value);

    public static ushort GetPc(this Cpu cpu) => cpu.ProgramCounter.Value;
    public static void SetPc(this Cpu cpu, ushort value) => cpu.ProgramCounter = new Unassigned16Bits(value);

    public static ushort GetSp(this Cpu cpu) => cpu.StackPointer.Value;
    public static void SetSp(this Cpu cpu, ushort value) => cpu.StackPointer = new Unassigned16Bits(value);

    public static Status GetFlags(this Cpu cpu) => cpu.Flags;
    public static void SetFlags(this Cpu cpu, Status value) => cpu.Flags = value;

    public static byte[] GetMemory(this Bus bus)
    {
        var memory = new byte[bus.Ram.Length];
        for (int i = 0; i < bus.Ram.Length; i++)
        {
            memory[i] = bus.Ram[i].Value;
        }
        return memory;
    }

    public static byte[] GetMemory(this Cpu cpu) => cpu.Bus.GetMemory();
}
