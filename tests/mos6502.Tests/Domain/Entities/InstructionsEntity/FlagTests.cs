namespace Mos6502.Tests.Domain.Entities;

public class FlagTests
{
  [Fact]
  public void Clc_ClearsCarryFlag()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = Status.Carry;

    var cycles = cpu.Clc();

    cpu.Status.HasFlag(Status.Carry).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Sec_SetsCarryFlag()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = 0;

    var cycles = cpu.Sec();

    cpu.Status.HasFlag(Status.Carry).Should().BeTrue();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Cli_ClearsInterruptFlag()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = Status.Interrupt;

    var cycles = cpu.Cli();

    cpu.Status.HasFlag(Status.Interrupt).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Sei_SetsInterruptFlag()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = 0;

    var cycles = cpu.Sei();

    cpu.Status.HasFlag(Status.Interrupt).Should().BeTrue();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Clv_ClearsOverflowFlag()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = Status.Overflow;

    var cycles = cpu.Clv();

    cpu.Status.HasFlag(Status.Overflow).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Cld_ClearsDecimalFlag()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = Status.Decimal;

    var cycles = cpu.Cld();

    cpu.Status.HasFlag(Status.Decimal).Should().BeFalse();
    cycles.Should().Be(2);
  }

  [Fact]
  public void Sed_SetsDecimalFlag()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.Status = 0;

    var cycles = cpu.Sed();

    cpu.Status.HasFlag(Status.Decimal).Should().BeTrue();
    cycles.Should().Be(2);
  }
}
