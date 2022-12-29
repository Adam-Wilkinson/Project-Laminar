using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.PluginFramework.UnitTests.NodeSystem.UnitTests;

public class ExecutionFlagsTests
{
    [Fact]
    public void ExecutionFlags_ShouldNotHaveTestFlagOne_WhenNoFlags()
    {
        ExecutionFlags sut = ExecutionFlags.None;

        TestFlagOne.HasTestFlagOne(sut).Should().BeFalse();
    }

    [Fact]
    public void ExecutionFlags_ShouldNotHaveTestFlagTwo_WhenNoFlags()
    {
        ExecutionFlags sut = ExecutionFlags.None;

        TestFlagTwo.HasTestFlagTwo(sut).Should().BeFalse();
    }

    [Fact]
    public void ExecutionFlags_ShouldHaveTestFlagOne_WhenOnlyFlagOne()
    {
        ExecutionFlags sut = TestFlagOne.Value;

        TestFlagOne.HasTestFlagOne(sut).Should().BeTrue();
    }

    [Fact]
    public void ExecutionFlags_ShouldHaveTestFlagTwo_WhenOnlyFlagTwo()
    {
        ExecutionFlags sut = TestFlagTwo.Value;

        TestFlagTwo.HasTestFlagTwo(sut).Should().BeTrue();
    }

    [Fact]
    public void ExecutionFlags_ShouldNotHaveTestFlagOne_WhenOnlyFlagTwo()
    {
        ExecutionFlags sut = TestFlagTwo.Value;

        TestFlagOne.HasTestFlagOne(sut).Should().BeFalse();
    }

    [Fact]
    public void ExecutionFlags_ShouldNotHaveTestFlagTwo_WhenOnlyFlagOne()
    {
        ExecutionFlags sut = TestFlagOne.Value;

        TestFlagTwo.HasTestFlagTwo(sut).Should().BeFalse();
    }

    [Fact]
    public void ExecutionFlags_ShouldHaveTestFlagOne_WhenBothFlags()
    {
        ExecutionFlags sut = TestFlagOne.Value + TestFlagTwo.Value;

        TestFlagOne.HasTestFlagOne(sut).Should().BeTrue();
    }

    [Fact]
    public void ExecutionFlags_ShouldHaveTestFlagTwo_WhenBothFlags()
    {
        ExecutionFlags sut = TestFlagOne.Value + TestFlagTwo.Value;

        TestFlagTwo.HasTestFlagTwo(sut).Should().BeTrue();
    }

    private static class TestFlagOne
    {
        public static readonly int Value = ExecutionFlags.ReserveNextFlagValue();

        public static bool HasTestFlagOne(ExecutionFlags executionFlags) => ExecutionFlags.HasFlag(executionFlags, Value);
    }

    private static class TestFlagTwo
    {
        public static readonly int Value = ExecutionFlags.ReserveNextFlagValue();

        public static bool HasTestFlagTwo(ExecutionFlags executionFlags) => ExecutionFlags.HasFlag(executionFlags, Value);
    }
}
