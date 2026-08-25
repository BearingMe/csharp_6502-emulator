namespace mos6502.Tests;

public class BranchTests
{
  [Fact]
  public void Bmi_BranchTaken_SamePage_UpdatesPcAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x80); // sets Negative flag

    var cycles = cpu.BMI_relative(0x05);

    cpu.PC.Should().Be(0x1005);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Bmi_BranchTaken_PageCrossing_UpdatesPcAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0xFE);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x80); // sets Negative flag

    var cycles = cpu.BMI_relative(0x05);

    cpu.PC.Should().Be(0x1103);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Bmi_BranchNotTaken_PreservesPcAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x01); // clears Negative flag

    var cycles = cpu.BMI_relative(0x05);

    cpu.PC.Should().Be(0x1000);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Bmi_NegativeOffset_BackwardBranch_Taken_UpdatesPcAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x20);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x80); // sets Negative flag

    var cycles = cpu.BMI_relative(unchecked((u8)(sbyte)-5));

    cpu.PC.Should().Be(0x101B);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Bmi_NegativeOffset_PageCrossingBackward_UpdatesPcAndReturnsFourCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x02);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x80); // sets Negative flag

    var cycles = cpu.BMI_relative(unchecked((u8)(sbyte)-5));

    cpu.PC.Should().Be(0x0FFD);
    cycles.Should().Be(4);
  }

  [Fact]
  public void Bpl_BranchTaken_WhenNegativeClear_UpdatesPcAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x01); // clears Negative flag

    var cycles = cpu.BPL_relative(0x05);

    cpu.PC.Should().Be(0x1005);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Bpl_BranchNotTaken_WhenNegativeSet_PreservesPcAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x80); // sets Negative flag

    var cycles = cpu.BPL_relative(0x05);

    cpu.PC.Should().Be(0x1000);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Beq_BranchTaken_WhenZeroSet_UpdatesPcAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x00); // sets Zero flag

    var cycles = cpu.BEQ_relative(0x05);

    cpu.PC.Should().Be(0x1005);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Beq_BranchNotTaken_WhenZeroClear_PreservesPcAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x01); // clears Zero flag

    var cycles = cpu.BEQ_relative(0x05);

    cpu.PC.Should().Be(0x1000);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Bne_BranchTaken_WhenZeroClear_UpdatesPcAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x01); // clears Zero flag

    var cycles = cpu.BNE_relative(0x05);

    cpu.PC.Should().Be(0x1005);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Bne_BranchNotTaken_WhenZeroSet_PreservesPcAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x00); // sets Zero flag

    var cycles = cpu.BNE_relative(0x05);

    cpu.PC.Should().Be(0x1000);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Bcs_BranchTaken_WhenCarrySet_UpdatesPcAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0xFF);
    cpu.ADC_immediate(0x01); // sets Carry flag

    var cycles = cpu.BCS_relative(0x05);

    cpu.PC.Should().Be(0x1005);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Bcs_BranchNotTaken_WhenCarryClear_PreservesPcAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x01);
    cpu.ADC_immediate(0x01); // clears Carry flag

    var cycles = cpu.BCS_relative(0x05);

    cpu.PC.Should().Be(0x1000);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Bcc_BranchTaken_WhenCarryClear_UpdatesPcAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x01);
    cpu.ADC_immediate(0x01); // clears Carry flag

    var cycles = cpu.BCC_relative(0x05);

    cpu.PC.Should().Be(0x1005);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Bcc_BranchNotTaken_WhenCarrySet_PreservesPcAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0xFF);
    cpu.ADC_immediate(0x01); // sets Carry flag

    var cycles = cpu.BCC_relative(0x05);

    cpu.PC.Should().Be(0x1000);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Bvs_BranchTaken_WhenOverflowSet_UpdatesPcAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x7F);
    cpu.ADC_immediate(0x01); // sets Overflow flag

    var cycles = cpu.BVS_relative(0x05);

    cpu.PC.Should().Be(0x1005);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Bvs_BranchNotTaken_WhenOverflowClear_PreservesPcAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x01);
    cpu.ADC_immediate(0x01); // clears Overflow flag

    var cycles = cpu.BVS_relative(0x05);

    cpu.PC.Should().Be(0x1000);
    cycles.Should().Be(2);
  }

  [Fact]
  public void Bvc_BranchTaken_WhenOverflowClear_UpdatesPcAndReturnsThreeCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x01);
    cpu.ADC_immediate(0x01); // clears Overflow flag

    var cycles = cpu.BVC_relative(0x05);

    cpu.PC.Should().Be(0x1005);
    cycles.Should().Be(3);
  }

  [Fact]
  public void Bvc_BranchNotTaken_WhenOverflowSet_PreservesPcAndReturnsTwoCycles()
  {
    var bus = new Bus();
    bus.WriteByte(0xFFFC, 0x00);
    bus.WriteByte(0xFFFD, 0x10);
    var cpu = new Emulator(bus);
    cpu.LDA_immediate(0x7F);
    cpu.ADC_immediate(0x01); // sets Overflow flag

    var cycles = cpu.BVC_relative(0x05);

    cpu.PC.Should().Be(0x1000);
    cycles.Should().Be(2);
  }
}
