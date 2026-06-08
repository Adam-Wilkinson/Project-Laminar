using Laminar.Domain.Notification.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentValue<T> : IObservableValue<T>
{
    public bool HasDefaultValue { get; }
    
    public T DefaultValue { get; }

    public void Reset();
}

public static class PersistentValueExtensions
{
    extension<T>(IPersistentValue<T> persistentValue)
    {
        public IPersistentValue<TOut> Cast<TOut>() => new CastPersistent<T, TOut>(persistentValue);
    }

    private class CastPersistent<TIn, TOut> : ObservableValueBase<TOut>, IPersistentValue<TOut>
    {
        private readonly IPersistentValue<TIn> _input;
        private TOut _value;
        
        public CastPersistent(IPersistentValue<TIn> input)
        {
            _input = input;
            DefaultValue = ForceCast(_input.DefaultValue);
            _value = ForceCast(_input.Value);

            input.OnChanged += (_, e) => Value = ForceCast(e.NewValue);
        }

        public override TOut Value { get => _value; set => SetAndRaise(ref _value, value); }

        public bool HasDefaultValue => _input.HasDefaultValue;
        
        public TOut DefaultValue { get; }

        public void Reset() => _input.Reset();

        protected override void AfterValueChanged()
        {
            _input.Value = Value is TIn input ? input : throw new InvalidCastException();
        }

        private static TOut ForceCast(TIn value) => value is TOut output ? output : throw new InvalidCastException();
    }
}