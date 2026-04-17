using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IObservableValueWithDefault<T> : IObservableValue<T>
{
    public T DefaultValue { get; }

    public void Reset();
}

public static class IObservableValueWithDefaultExtensions
{
    extension<T>(IObservableValueWithDefault<T> observableValue)
    {
        public IObservableValueWithDefault<TOut> Cast<TOut>() => new CastObservable<T, TOut>(observableValue);
    }

    private class CastObservable<TIn, TOut> : ObservableValueBase<TOut>, IObservableValueWithDefault<TOut>
    {
        private readonly IObservableValueWithDefault<TIn> _input;
        private TOut _value;
        
        public CastObservable(IObservableValueWithDefault<TIn> input)
        {
            _input = input;
            DefaultValue = ForceCast(_input.DefaultValue);
            _value = ForceCast(_input.Value);

            input.OnChanged += (_, e) => Value = ForceCast(e.NewValue);
        }

        public override TOut Value { get => _value; set => SetAndRaise(ref _value, value); }

        public TOut DefaultValue { get; }

        public void Reset() => _input.Reset();

        protected override void OnValueChanged()
        {
            _input.Value = Value is TIn input ? input : throw new InvalidCastException();
        }

        private static TOut ForceCast(TIn value) => value is TOut output ? output : throw new InvalidCastException();
    }
}