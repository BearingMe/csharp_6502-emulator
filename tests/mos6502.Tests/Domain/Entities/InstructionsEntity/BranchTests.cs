namespace Mos6502.Tests.Domain.Entities;

public class BranchTests
{
  [Theory]
  [InlineData(Status.Carry, 0x10, true, 3)]
  [InlineData((Status)0, 0x10, false, 2)]
  public void Bcs_Branches_WhenCarryIsSet(Status status, i8 offset, bool branched, cycle expectedCycles)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x8000;
    cpu.Status = status;

    var cycles = cpu.BcsRelative((u8)offset);

    cpu.PC.Should().Be((u16)(branched ? 0x8000 + offset : 0x8000));
    cycles.Should().Be(expectedCycles);
  }

  [Theory]
  [InlineData((Status)0, 0x10, true, 3)]
  [InlineData(Status.Carry, 0x10, false, 2)]
  public void Bcc_Branches_WhenCarryIsClear(Status status, i8 offset, bool branched, cycle expectedCycles)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x8000;
    cpu.Status = status;

    var cycles = cpu.BccRelative((u8)offset);

    cpu.PC.Should().Be((u16)(branched ? 0x8000 + offset : 0x8000));
    cycles.Should().Be(expectedCycles);
  }

  [Theory]
  [InlineData(Status.Zero, 0x10, true, 3)]
  [InlineData((Status)0, 0x10, false, 2)]
  public void Beq_Branches_WhenZeroIsSet(Status status, i8 offset, bool branched, cycle expectedCycles)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x8000;
    cpu.Status = status;

    var cycles = cpu.BeqRelative((u8)offset);

    cpu.PC.Should().Be((u16)(branched ? 0x8000 + offset : 0x8000));
    cycles.Should().Be(expectedCycles);
  }

  [Theory]
  [InlineData((Status)0, 0x10, true, 3)]
  [InlineData(Status.Zero, 0x10, false, 2)]
  public void Bne_Branches_WhenZeroIsClear(Status status, i8 offset, bool branched, cycle expectedCycles)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x8000;
    cpu.Status = status;

    var cycles = cpu.BneRelative((u8)offset);

    cpu.PC.Should().Be((u16)(branched ? 0x8000 + offset : 0x8000));
    cycles.Should().Be(expectedCycles);
  }

  [Theory]
  [InlineData(Status.Negative, 0x10, true, 3)]
  [InlineData((Status)0, 0x10, false, 2)]
  public void Bmi_Branches_WhenNegativeIsSet(Status status, i8 offset, bool branched, cycle expectedCycles)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x8000;
    cpu.Status = status;

    var cycles = cpu.BmiRelative((u8)offset);

    cpu.PC.Should().Be((u16)(branched ? 0x8000 + offset : 0x8000));
    cycles.Should().Be(expectedCycles);
  }

  [Theory]
  [InlineData((Status)0, 0x10, true, 3)]
  [InlineData(Status.Negative, 0x10, false, 2)]
  public void Bpl_Branches_WhenNegativeIsClear(Status status, i8 offset, bool branched, cycle expectedCycles)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x8000;
    cpu.Status = status;

    var cycles = cpu.BplRelative((u8)offset);

    cpu.PC.Should().Be((u16)(branched ? 0x8000 + offset : 0x8000));
    cycles.Should().Be(expectedCycles);
  }

  [Theory]
  [InlineData(Status.Overflow, 0x10, true, 3)]
  [InlineData((Status)0, 0x10, false, 2)]
  public void Bvs_Branches_WhenOverflowIsSet(Status status, i8 offset, bool branched, cycle expectedCycles)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x8000;
    cpu.Status = status;

    var cycles = cpu.BvsRelative((u8)offset);

    cpu.PC.Should().Be((u16)(branched ? 0x8000 + offset : 0x8000));
    cycles.Should().Be(expectedCycles);
  }

  [Theory]
  [InlineData((Status)0, 0x10, true, 3)]
  [InlineData(Status.Overflow, 0x10, false, 2)]
  public void Bvc_Branches_WhenOverflowIsClear(Status status, i8 offset, bool branched, cycle expectedCycles)
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x8000;
    cpu.Status = status;

    var cycles = cpu.BvcRelative((u8)offset);

    cpu.PC.Should().Be((u16)(branched ? 0x8000 + offset : 0x8000));
    cycles.Should().Be(expectedCycles);
  }

  [Fact]
  public void Branch_AddsCycle_WhenPageBoundaryIsCrossed()
  {
    var bus = new Bus();
    var cpu = new Mos6502.Application.Emulator(bus);
    cpu.PC = 0x80FE;
    cpu.Status = Status.Carry;

    var cycles = cpu.BcsRelative(0x04); // target is 0x8102, crosses page 0x80 -> 0x81

    cpu.PC.Should().Be(0x8102);
    cycles.Should().Be(4);
  }
}
