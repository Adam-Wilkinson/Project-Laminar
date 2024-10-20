using FluentAssertions;
using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.UnitTests.ValueObjects.UnitTests;

public class OpacityTests
{
     readonly Opacity _sut = new();

    [Fact]
    public void ShouldBeOne_ByDefault()
    {
        _sut.Value.Should().Be(1);
    }

    public class SetInternalValue
    {
        readonly Opacity _sut = new();

        [Fact]
        public void ShouldAdjustValue_WhenCalled()
        {
            _sut.SetInternalValue(0.5);

            _sut.Value.Should().Be(0.5);
        }

        [Fact]
        public void ShouldRaisePropertyChanged_WhenCalled()
        {
            using var mon = _sut.Monitor();

            _sut.SetInternalValue(0.5);

            mon.Should().RaisePropertyChangeFor(opacity => opacity.Value);
        }
    }

    public class AddFactor
    {
        readonly Opacity _sut = new();

        [Fact]
        public void ShouldChangeValue_WhenFactorAdded()
        {
            Opacity factor = new();
            factor.SetInternalValue(0.25);

            _sut.AddFactor(factor);

            _sut.Value.Should().Be(0.25);
        }

        [Fact]
        public void ShouldRaisePropertyChanged_WhenFactorAdded()
        {
            Opacity factor = new();
            factor.SetInternalValue(0.25);
            using var mon = _sut.Monitor();

            _sut.AddFactor(factor);

            mon.Should().RaisePropertyChangeFor(opacity => opacity.Value);
        }

        [Fact]
        public void ShouldHaveCorrectValue_WhenFactorValueChanged()
        {
            Opacity factor = new();
            factor.SetInternalValue(0.25);
            _sut.AddFactor(factor);

            factor.SetInternalValue(0.3);

            _sut.Value.Should().Be(0.3);
        }

        [Fact]
        public void ShouldRaisePropertyChanged_WhenFactorChanged()
        {
            Opacity factor = new();
            factor.SetInternalValue(0.25);
            _sut.AddFactor(factor);
            using var mon = _sut.Monitor();

            factor.SetInternalValue(0.3);

            mon.Should().RaisePropertyChangeFor(opacity => opacity.Value);
        }
    }

    public class RemoveFactor
    {
        readonly Opacity _sut = new();
        readonly Opacity _factor = new(0.25);

        [Fact]
        public void ShouldHaveCorrectValue_WhenFactorRemoved()
        {
            _sut.AddFactor(_factor);

            _sut.RemoveFactor(_factor);

            _sut.Value.Should().Be(1);
        }

        [Fact]
        public void ShouldRaisePropertyChanged_WhenFactorRemoved()
        {
            _sut.AddFactor(_factor);
            using var mon = _sut.Monitor();

            _sut.RemoveFactor(_factor);

            mon.Should().RaisePropertyChangeFor(opacity => opacity.Value);
        }

        [Fact]
        public void ShouldNotChangeValue_WhenRemovedFactorChanged()
        {
            _sut.AddFactor(_factor);
            _sut.RemoveFactor(_factor);

            double valueBefore = _sut.Value;
            _factor.SetInternalValue(0.3);

            _sut.Value.Should().Be(valueBefore);
        }

        [Fact]
        public void ShouldNotRaisePropertyChanged_WhenRemovedFactorChanged()
        {
            _sut.AddFactor(_factor);
            _sut.RemoveFactor(_factor);
            using var mon = _sut.Monitor();

            _factor.SetInternalValue(0.3);

            mon.Should().NotRaisePropertyChangeFor(opacity => opacity.Value);
       }
    }
}
