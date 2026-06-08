using Laminar.Domain.Notification.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.UnitTests.ValueObjects.UnitTests;

public class ObservableValueTests
{
    public class CastTests
    {
        [Fact]
        public void ShouldInitializeProperly()
        {
            IReadOnlyObservableValue<object> sut = new ObservableValue<object>(5);

            IReadOnlyObservableValue<int> castedValue = sut.Cast<object, int>();
            
            castedValue.Value.Should().Be(5);
        }
        
        [Fact]
        public void ShouldReactToValueChanged()
        {
            ObservableValue<object> sut = new(5);
            IReadOnlyObservableValue<int> castedValue = sut.Cast<object, int>();
            using var mon = castedValue.Monitor();
            
            sut.Value = 10;
            
            castedValue.Value.Should().Be(10);
            mon.Should().RaisePropertyChangeFor(x => x.Value);
            mon.Should().Raise(nameof(ICovariantObservableValue<>.CovariantOnChanged));
            mon.Should().Raise(nameof(IReadOnlyObservableValue<>.OnChanged))
                .WithSender(castedValue)
                .WithArgs<ObservableValueChangedEventArgs<int>>(
                    x => x.OldValue == 5 && x.NewValue == 10);
        }

        [Fact]
        public void ShouldReactToInvalidCast()
        {
            ObservableValue<object> sut = new(5);
            IReadOnlyObservableValue<int> castedValue = sut.Cast<object, int>();
            
            sut.Invoking(x => x.Value = "Not an integer").Should().Throw<InvalidCastException>();
        }
    }
}