namespace Mos6502.Tests.Domain.Entities;

public class BitTests
{
  [Fact]
  public void Bit_SetsZeroFlag_WhenAndResultIsZero()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x0F);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0xF0);

    var cycles = cpu.BitZeroPage(0x42);

    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
    cycles.Should().Be(3);
  }

  [Fact]
  public void Bit_ClearsZeroFlag_WhenAndResultIsNonZero()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0xFF);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x01);

    cpu.BitZeroPage(0x42);

    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
  }

  [Fact]
  public void Bit_TransfersBits6And7_ToOverflowAndNegativeFlags()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0xC0); // bits 6 and 7 set
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x00);      // A is 0, so AND result will be 0 (Zero=true)

    cpu.BitZeroPage(0x42);

    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
    cpu.Status.HasFlag(Status.Overflow).Should().BeTrue();
    cpu.Status.HasFlag(Status.Zero).Should().BeTrue();
  }

  [Fact]
  public void Bit_ClearsOverflowAndNegativeFlags_WhenBits6And7AreZero()
  {
    var bus = new Bus();
    bus.WriteByte(0x0042, 0x3F); // bits 6 and 7 clear
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x01);

    cpu.BitZeroPage(0x42);

    cpu.Status.HasFlag(Status.Negative).Should().BeFalse();
    cpu.Status.HasFlag(Status.Overflow).Should().BeFalse();
  }

  [Fact]
  public void Bit_Absolute_ReadsMemoryAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0x1234, 0x80);
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.LdaImmediate(0x80);

    var cycles = cpu.BitAbsolute(0x1234);

    cpu.Status.HasFlag(Status.Negative).Should().BeTrue();
    cpu.Status.HasFlag(Status.Overflow).Should().BeFalse();
    cpu.Status.HasFlag(Status.Zero).Should().BeFalse();
    cycles.Should().Be(4);
  }
}
