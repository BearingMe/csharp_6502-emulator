namespace Mos6502.Tests.Application.Emulator.Functional;

public class FunctionalArithmeticStackPipelineTests
{
  [Fact]
  public void ArithmeticAndStackPipeline_ExecutesSequenceCorrectly()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x80);

    var emulator = new Mos6502.Application.Emulator(bus);

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

    emulator.LoadRom(program, 0x8000);
    emulator.Reset();

    var step1Cycles = emulator.Step(); // LDA #$10
    emulator.A.Should().Be(0x10);
    emulator.PC.Should().Be(0x8002);
    step1Cycles.Should().Be(2);

    var step2Cycles = emulator.Step(); // TAX
    emulator.X.Should().Be(0x10);
    emulator.PC.Should().Be(0x8003);
    step2Cycles.Should().Be(2);

    var step3Cycles = emulator.Step(); // INX
    emulator.X.Should().Be(0x11);
    emulator.PC.Should().Be(0x8004);
    step3Cycles.Should().Be(2);

    var step4Cycles = emulator.Step(); // ADC #$05
    emulator.A.Should().Be(0x15);
    emulator.PC.Should().Be(0x8006);
    step4Cycles.Should().Be(2);

    var step5Cycles = emulator.Step(); // PHA
    emulator.StackPointer.Should().Be(0xF9);
    bus.ReadByte(0x01FA).Should().Be(0x15);
    emulator.PC.Should().Be(0x8007);
    step5Cycles.Should().Be(3);

    var step6Cycles = emulator.Step(); // PLA
    emulator.A.Should().Be(0x15);
    emulator.StackPointer.Should().Be(0xFA);
    emulator.PC.Should().Be(0x8008);
    step6Cycles.Should().Be(4);

    var step7Cycles = emulator.Step(); // STA $0200
    bus.ReadByte(0x0200).Should().Be(0x15);
    emulator.PC.Should().Be(0x800B);
    step7Cycles.Should().Be(4);

    var totalCycles = step1Cycles + step2Cycles + step3Cycles + step4Cycles + step5Cycles + step6Cycles + step7Cycles;
    totalCycles.Should().Be(19);
  }
}
