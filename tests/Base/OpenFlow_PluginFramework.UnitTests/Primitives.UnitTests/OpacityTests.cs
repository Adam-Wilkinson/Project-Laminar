using FluentAssertions;
using OpenFlow_Core.Primitives;
using Xunit;

namespace OpenFlow_Core.UnitTests.Primitives.UnitTests
{
    public class OpacityTests
    {
        private readonly Opacity _sut = new();

        [Fact]
        public void Value_ShouldSetProperly_WhenNoFactors()
        {
            using (var monitoredSUT = _sut.Monitor())
            {
                _sut.Value = 0.5;
                monitoredSUT.Should().RaisePropertyChangeFor(x => x.Value);
            }
            _sut.Value.Should().Be(0.5);
        }

        [Fact]
        public void AddOpacityFactor_ShouldAddFactor()
        {
            using (var monitoredSUT = _sut.Monitor())
            {
                _sut.AddOpacityFactor(new Opacity() { Value = 0.5 });
                monitoredSUT.Should().RaisePropertyChangeFor(x => x.Value);
            }
            _sut.Value.Should().Be(0.5);
        }

        [Fact]
        public void RemoveOpacityFactor_ShouldRemoveFactor_WhenFactorContained()
        {
            Opacity factor = new() { Value = 0.5 };
            _sut.AddOpacityFactor(factor);
            bool factorRemoved;
            using (var monitoredSUT = _sut.Monitor())
            {
                factorRemoved = _sut.RemoveOpacityFactor(factor);
                monitoredSUT.Should().RaisePropertyChangeFor(x => x.Value);
            }
            _sut.Value.Should().Be(1.0);
            factorRemoved.Should().BeTrue();
        }

        [Fact]
        public void RemoveOpacityFactor_ShouldBeFalse_WhenFactorNotContained()
        {
            bool factorRemoved;
            using (var monitoredSUT = _sut.Monitor())
            {
                factorRemoved = _sut.RemoveOpacityFactor(new Opacity());
                monitoredSUT.Should().NotRaisePropertyChangeFor(x => x.Value);
            }
            factorRemoved.Should().BeFalse();
        }

        [Fact]
        public void Value_ShouldSetProperly_WhenItHasFactors()
        {
            _sut.AddOpacityFactor(new Opacity() { Value = 0.5 });
            using (var monitoredSUT = _sut.Monitor())
            {
                _sut.Value = 0.5;
                monitoredSUT.Should().RaisePropertyChangeFor(x => x.Value);
            }
            _sut.Value.Should().Be(0.25);
        }

        [Fact]
        public void Value_ShouldRelayPropertyChanged_WhenFactorPropertyChanged()
        {
            Opacity factor = new();
            _sut.AddOpacityFactor(factor);
            using var monitoredSUT = _sut.Monitor();
            factor.Value = 0.5;
            monitoredSUT.Should().RaisePropertyChangeFor(x => x.Value);
        }

        [Fact]
        public void Value_ShouldChange_WhenFactorPropertyChanged()
        {
            Opacity factor = new();
            _sut.AddOpacityFactor(factor);
            factor.Value = 0.5;
            _sut.Value.Should().Be(0.5);
        }
    }
}
