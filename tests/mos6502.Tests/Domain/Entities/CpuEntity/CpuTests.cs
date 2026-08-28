namespace Mos6502.Tests.Domain.Entities;

public class CpuTests
{
  [Fact]
  public void Constructor_InitializesRegistersToDefaultResetState()
  {
    var cpu = new Mos6502.Domain.Entities.Cpu();

    cpu.A.Should().Be(0x00);
    cpu.X.Should().Be(0x00);
    cpu.Y.Should().Be(0x00);
    cpu.StackPointer.Should().Be(0xFD);
    cpu.PC.Should().Be(0x0000);
    cpu.Status.Should().Be(Status.Interrupt);
  }

  [Fact]
  public void Reset_SetsProvidedResetVectorAndInterruptFlag()
  {
    var cpu = new Mos6502.Domain.Entities.Cpu();
    cpu.A = 0x55;
    cpu.X = 0x66;
    cpu.Y = 0x77;
    cpu.StackPointer = 0x80;
    cpu.Status = Status.Carry | Status.Zero;

    cpu.Reset(0xC000);

    cpu.A.Should().Be(0x00);
    cpu.X.Should().Be(0x00);
    cpu.Y.Should().Be(0x00);
    cpu.StackPointer.Should().Be(0xFD);
    cpu.PC.Should().Be(0xC000);
    cpu.Status.Should().Be(Status.Interrupt);
  }
}
