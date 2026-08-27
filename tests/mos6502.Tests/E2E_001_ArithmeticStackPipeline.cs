namespace mos6502.Tests;

public class E2E_001_ArithmeticStackPipeline
{
  [Fact]
  public void ArithmeticAndStackPipeline_ExecutesSequenceCorrectly()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    var cpu = new Emulator(bus);

    byte[] program =
    [
      0xA9, 0x10,       // LDA #$10
      0xAA,             // TAX
      0xE8,             // INX
      0x69, 0x05,       // ADC #$05
      0x48,             // PHA
      0x68,             // PLA
      0x8D, 0x00, 0x02  // STA $0200
    ];

    cpu.LoadRom(program, 0x8000);
    cpu.Reset();

    var step1Cycles = cpu.Step(); // LDA #$10
    cpu.A.Should().Be(0x10);
    cpu.PC.Should().Be(0x8002);
    step1Cycles.Should().Be(2);

    var step2Cycles = cpu.Step(); // TAX
    cpu.X.Should().Be(0x10);
    cpu.PC.Should().Be(0x8003);
    step2Cycles.Should().Be(2);

    var step3Cycles = cpu.Step(); // INX
    cpu.X.Should().Be(0x11);
    cpu.PC.Should().Be(0x8004);
    step3Cycles.Should().Be(2);

    var step4Cycles = cpu.Step(); // ADC #$05
    cpu.A.Should().Be(0x15);
    cpu.PC.Should().Be(0x8006);
    step4Cycles.Should().Be(2);

    var step5Cycles = cpu.Step(); // PHA
    cpu.StackPointer.Should().Be(0xF9);
    bus.ReadByte(0x01FA).Should().Be(0x15);
    cpu.PC.Should().Be(0x8007);
    step5Cycles.Should().Be(3);

    var step6Cycles = cpu.Step(); // PLA
    cpu.A.Should().Be(0x15);
    cpu.StackPointer.Should().Be(0xFA);
    cpu.PC.Should().Be(0x8008);
    step6Cycles.Should().Be(4);

    var step7Cycles = cpu.Step(); // STA $0200
    bus.ReadByte(0x0200).Should().Be(0x15);
    cpu.PC.Should().Be(0x800B);
    step7Cycles.Should().Be(4);

    var totalCycles = step1Cycles + step2Cycles + step3Cycles + step4Cycles + step5Cycles + step6Cycles + step7Cycles;
    totalCycles.Should().Be(19);
  }
}
