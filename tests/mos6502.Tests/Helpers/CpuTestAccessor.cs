using System.Reflection;
using mos6502.src;

namespace mos6502.Tests.Helpers;

internal static class CpuTestAccessor
{
    private static readonly FieldInfo AField = typeof(mos6502.src.Cpu).GetField("a", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo XField = typeof(mos6502.src.Cpu).GetField("x", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo YField = typeof(mos6502.src.Cpu).GetField("y", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo PcField = typeof(mos6502.src.Cpu).GetField("pc", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo SpField = typeof(mos6502.src.Cpu).GetField("sp", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo FlagsField = typeof(mos6502.src.Cpu).GetField("flags", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo MemoryField = typeof(mos6502.src.Cpu).GetField("memory", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public static byte GetA(this mos6502.src.Cpu cpu) => (byte)AField.GetValue(cpu)!;
    public static void SetA(this mos6502.src.Cpu cpu, byte value) => AField.SetValue(cpu, value);

    public static byte GetX(this mos6502.src.Cpu cpu) => (byte)XField.GetValue(cpu)!;
    public static void SetX(this mos6502.src.Cpu cpu, byte value) => XField.SetValue(cpu, value);

    public static byte GetY(this mos6502.src.Cpu cpu) => (byte)YField.GetValue(cpu)!;
    public static void SetY(this mos6502.src.Cpu cpu, byte value) => YField.SetValue(cpu, value);

    public static ushort GetPc(this mos6502.src.Cpu cpu) => (ushort)PcField.GetValue(cpu)!;
    public static void SetPc(this mos6502.src.Cpu cpu, ushort value) => PcField.SetValue(cpu, value);

    public static ushort GetSp(this mos6502.src.Cpu cpu) => (ushort)SpField.GetValue(cpu)!;
    public static void SetSp(this mos6502.src.Cpu cpu, ushort value) => SpField.SetValue(cpu, value);

    public static Status GetFlags(this mos6502.src.Cpu cpu) => (Status)FlagsField.GetValue(cpu)!;
    public static void SetFlags(this mos6502.src.Cpu cpu, Status value) => FlagsField.SetValue(cpu, value);

    public static byte[] GetMemory(this mos6502.src.Cpu cpu) => (byte[])MemoryField.GetValue(cpu)!;
}
