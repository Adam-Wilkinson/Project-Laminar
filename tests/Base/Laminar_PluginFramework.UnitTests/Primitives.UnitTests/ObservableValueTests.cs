using FluentAssertions;
using Laminar_Core.Primitives;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Laminar_Core.UnitTests.Primitives.UnitTests
{
    public class ObservableValueTests
    {
        private const string ValidTestString = "This is a test";
        private readonly ObservableValue<string> _sut = new();

        [Fact]
        public void Value_ShouldChange_WhenInputValid()
        {
            _sut.Value = ValidTestString;
            _sut.Value.Should().Be(ValidTestString);
        }

        [Fact]
        public void Value_ShouldNotChange_WhenSetToNull()
        {
            _sut.Value = ValidTestString;
            _sut.Value = null;

            _sut.Value.Should().Be(ValidTestString);
        }

        [Fact]
        public void Value_ShouldRaiseEvent_WhenChanged()
        {
            using var monitor = _sut.Monitor();
            _sut.Value = ValidTestString;
            monitor.Should().RaisePropertyChangeFor(x => x.Value);
            monitor.Should().Raise(nameof(ObservableValue<string>.OnChange));
        }

        [Fact]
        public void Value_ShouldNotRaiseEvent_WhenUnchanged()
        {
            _sut.Value = ValidTestString;
            using var monitor = _sut.Monitor();
            _sut.Value = ValidTestString;
            monitor.Should().NotRaisePropertyChangeFor(x => x.Value);
            monitor.Should().NotRaise(nameof(ObservableValue<string>.OnChange));
        }

        [Fact]
        public void Clone_ShouldHaveSameValue()
        {
            _sut.Value = ValidTestString;
            IObservableValue<string> newValue = _sut.Clone();
            newValue.Value.Should().Be(ValidTestString);
        }
    }
}
