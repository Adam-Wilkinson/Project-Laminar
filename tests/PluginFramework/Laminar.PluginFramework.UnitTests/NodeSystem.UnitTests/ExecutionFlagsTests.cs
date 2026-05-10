using FluentAssertions;
using Laminar.PluginFramework.NodeSystem;
using Xunit;

namespace Laminar.PluginFramework.UnitTests.NodeSystem.UnitTests;

public class ExecutionFlagsTests
{
    [Fact]
    public void ExecutionFlags_ShouldNotHaveTestFlagOne_WhenNoFlags()
    {
        ExecutionFlags sut = ExecutionFlags.None;
        sut.HasFlag(TestFlagOne.Value).Should().BeFalse();
    }

    [Fact]
    public void ExecutionFlags_ShouldNotHaveTestFlagTwo_WhenNoFlags()
    {
        ExecutionFlags sut = ExecutionFlags.None;
        sut.HasFlag(TestFlagTwo.Value).Should().BeFalse();
    }

    [Fact]
    public void ExecutionFlags_ShouldHaveTestFlagOne_WhenOnlyFlagOne()
    {
        ExecutionFlags sut = TestFlagOne.Value;
        sut.HasFlag(TestFlagOne.Value).Should().BeTrue();
    }

    [Fact]
    public void ExecutionFlags_ShouldHaveTestFlagTwo_WhenOnlyFlagTwo()
    {
        ExecutionFlags sut = TestFlagTwo.Value;
        sut.HasFlag(TestFlagTwo.Value).Should().BeTrue();
    }

    [Fact]
    public void ExecutionFlags_ShouldNotHaveTestFlagOne_WhenOnlyFlagTwo()
    {
        ExecutionFlags sut = TestFlagTwo.Value;
        sut.HasFlag(TestFlagOne.Value).Should().BeFalse();
    }

    [Fact]
    public void ExecutionFlags_ShouldNotHaveTestFlagTwo_WhenOnlyFlagOne()
    {
        ExecutionFlags sut = TestFlagOne.Value;
        sut.HasFlag(TestFlagTwo.Value).Should().BeFalse();
    }

    [Fact]
    public void ExecutionFlags_ShouldHaveTestFlagOne_WhenBothFlags()
    {
        ExecutionFlags sut = TestFlagOne.Value | TestFlagTwo.Value;
        sut.HasFlag(TestFlagOne.Value).Should().BeTrue();
    }

    [Fact]
    public void ExecutionFlags_ShouldHaveTestFlagTwo_WhenBothFlags()
    {
        ExecutionFlags sut = TestFlagOne.Value | TestFlagTwo.Value;
        sut.HasFlag(TestFlagTwo.Value).Should().BeTrue();
    }

    private static class TestFlagOne
    {
        public static readonly ExecutionFlags Value = ExecutionFlags.ReserveNext();
    }

    private static class TestFlagTwo
    {
        public static readonly ExecutionFlags Value = ExecutionFlags.ReserveNext();
    }
}
