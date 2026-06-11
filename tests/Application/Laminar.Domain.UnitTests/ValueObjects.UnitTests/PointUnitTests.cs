using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.UnitTests.ValueObjects.UnitTests;

public class PointUnitTests
{
    public class ToStringOverride
    {
        [Fact]
        public void ShouldFormatCorrectly()
        {
            Point sut =  new() {X = 1, Y = 2};
            sut.ToString().Should().Be("(1, 2)");
        }
    }

    public class Parse
    {
        [Fact]
        public void ShouldParseCorrectly()
        {
            Point sut = Point.Parse("(1, 2)");
            sut.X.Should().Be(1);
            sut.Y.Should().Be(2);
        }

        [Fact]
        public void ShouldParseOwnToString()
        {
            Point sut = new() { X = 12, Y = 631.3 };
            Point.Parse(sut.ToString()).Should().Be(sut);
        }
    }
}