global using u8 = byte;
global using i8 = sbyte;
global using u16 = ushort;
global using i16 = short;
global using cycle = int;

using System.Diagnostics;
using mos6502;
using mos6502.src.Domain.Enums;

Console.WriteLine("MOS 6502 Functional Test Runner");

var romPath = Path.Combine(AppContext.BaseDirectory, "assets", "roms", "nestest.nes");
if (!File.Exists(romPath))
{
  romPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "roms", "nestest.nes");
}

if (!File.Exists(romPath))
{
  Console.Error.WriteLine($"Error: ROM not found at '{romPath}'");
  return 1;
}

Console.WriteLine($"Loading ROM from: {romPath}");
var fileData = File.ReadAllBytes(romPath);

// iNES format handling: 16-byte header, 16KB PRG-ROM
byte[] prgRom;
if (fileData.Length >= 16 && fileData[0] == 'N' && fileData[1] == 'E' && fileData[2] == 'S' && fileData[3] == 0x1A)
{
  var prgRomSize = fileData[4] * 16384;
  prgRom = new byte[prgRomSize];
  Array.Copy(fileData, 16, prgRom, 0, prgRomSize);
}
else
{
  prgRom = fileData;
}

var bus = new Bus();
var cpu = new Emulator(bus);

if (prgRom.Length == 16384)
{
  // 16KB PRG-ROM mirrored to 0x8000 and 0xC000
  cpu.LoadRom(prgRom, 0x8000);
  cpu.LoadRom(prgRom, 0xC000);
}
else
{
  cpu.LoadRom(prgRom, (u16)(0x10000 - prgRom.Length));
}

// nestest automation starts at 0xC000
cpu.PC = 0xC000;
cpu.Status = Status.Unused | Status.Interrupt;
cpu.StackPointer = 0xFD;

Console.WriteLine($"Starting execution at PC = 0x{cpu.PC:X4}...");

ulong totalCycles = 0;
ulong totalInstructions = 0;
var stopwatch = Stopwatch.StartNew();

u16 previousPc = 0xFFFF;
var samePcCount = 0;

while (true)
{
  var currentPc = cpu.PC;

  var err02 = bus.ReadByte(0x0002);
  var err03 = bus.ReadByte(0x0003);

  // When official tests complete, PC hits C6BD (start of unofficial opcodes) or official completion routine
  if (currentPc == 0xC6BD)
  {
    stopwatch.Stop();
    Console.WriteLine();
    if (err02 == 0)
    {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"*** SUCCESS! Official 6502 instructions test passed ($02 = 0x00) at PC = 0x{currentPc:X4} ***");
    }
    else
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine($"*** FAILED: $02 = 0x{err02:X2} at PC = 0x{currentPc:X4} ***");
    }
    Console.ResetColor();
    Console.WriteLine($"A = 0x{cpu.A:X2}, X = 0x{cpu.X:X2}, Y = 0x{cpu.Y:X2}, SP = 0x{cpu.StackPointer:X2}, Status = {cpu.Status} (0x{(byte)cpu.Status:X2})");
    Console.WriteLine($"Instructions executed: {totalInstructions:N0}");
    Console.WriteLine($"Cycles executed:       {totalCycles:N0}");
    Console.WriteLine($"Elapsed time:          {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
    return err02 == 0 ? 0 : 1;
  }

  // Check if trapped
  if (currentPc == previousPc)
  {
    samePcCount++;
    if (samePcCount > 2)
    {
      stopwatch.Stop();
      Console.WriteLine();
      if (err02 == 0)
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"*** SUCCESS! Official opcodes passed ($02 = 0x00, $03 = 0x{err03:X2}) at PC = 0x{currentPc:X4} ***");
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"*** FAILED: $02 = 0x{err02:X2}, $03 = 0x{err03:X2} at PC = 0x{currentPc:X4} ***");
      }
      Console.ResetColor();
      Console.WriteLine($"A = 0x{cpu.A:X2}, X = 0x{cpu.X:X2}, Y = 0x{cpu.Y:X2}, SP = 0x{cpu.StackPointer:X2}, Status = {cpu.Status} (0x{(byte)cpu.Status:X2})");
      Console.WriteLine($"Instructions executed: {totalInstructions:N0}");
      Console.WriteLine($"Cycles executed:       {totalCycles:N0}");
      Console.WriteLine($"Elapsed time:          {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
      return err02 == 0 ? 0 : 1;
    }
  }
  else
  {
    samePcCount = 0;
    previousPc = currentPc;
  }

  var cycles = cpu.Step();
  totalCycles += (ulong)cycles;
  totalInstructions++;
}
