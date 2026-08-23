namespace mos6502;

/// <summary>
/// Provides the 6502's 64 KiB addressable memory space.
/// </summary>
public class Bus
{
  private readonly u8[] _ram;

  public Bus()
  {
    _ram = new u8[0x10000]; // 64 KiB
  }

  /// <summary>
  /// Writes a byte to the specified memory address.
  /// </summary>
  public void WriteByte(u16 address, u8 data) => _ram[address] = data;

  /// <summary>
  /// Reads a byte from the specified memory address.
  /// </summary>
  public u8 ReadByte(u16 address) => _ram[address];

  /// <summary>
  /// Reads two consecutive bytes and combines them into a 16-bit value.
  /// </summary>
  public u16 ReadWord(u16 address)
  {
    var lo = _ram[address];
    var hi = _ram[(u16)(address + 1)];

    return (u16)(lo | hi << 8);
  }
}