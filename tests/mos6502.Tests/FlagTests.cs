namespace mos6502.Tests;

public class FlagTests
{
  [Fact]
  public void Clc_ClearsCarryFlagAndReturnsTwoCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.SEC();

    var cycles = cpu.CLC();

    cpu.Status.HasFlag(Status.Carry).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Sec_SetsCarryFlagAndReturnsTwoCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.CLC();

    var cycles = cpu.SEC();

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Cli_ClearsInterruptDisableFlagAndReturnsTwoCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.SEI();

    var cycles = cpu.CLI();

    cpu.Status.HasFlag(Status.Interrupt).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Sei_SetsInterruptDisableFlagAndReturnsTwoCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.CLI();

    var cycles = cpu.SEI();

    cpu.Status.HasFlag(Status.Interrupt).Should().BeTrue();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Clv_ClearsOverflowFlagAndReturnsTwoCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x7F);
    cpu.ADC_immediate(0x01); // sets Overflow flag

    var cycles = cpu.CLV();

    cpu.Status.HasFlag(Status.Overflow).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Cld_ClearsDecimalFlagAndReturnsTwoCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.SED();

    var cycles = cpu.CLD();

    cpu.Status.HasFlag(Status.Decimal).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Sed_SetsDecimalFlagAndReturnsTwoCycles()
  {
    var bus = new Bus();
    var cpu = new Emulator(bus);
    cpu.CLD();

    var cycles = cpu.SED();

    cpu.Status.HasFlag(Status.Decimal).Should().BeTrue();
    cycles.Should().Be(2);
  }
}
