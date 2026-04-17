using System.ComponentModel;

namespace Laminar.Domain.ValueObjects;

public interface IObservableValue<T> : IReadOnlyObservableValue<T>, IValueSink<T>
{   
    public new T Value { get; set; }
}

public interface IReadOnlyObservableValue<T> : ICovariantObservableValue<T>
{
    public event EventHandler<ObservableValueChangedEventArgs<T>>? OnChanged;
}

public interface ICovariantObservableValue<out T> : IObservableValueBase
{
    public T Value { get; }

    public event EventHandler CovariantOnChanged;
}

public interface IObservableValueBase : INotifyPropertyChanged
{
    public static readonly PropertyChangedEventArgs ValueChangedEventArgs = new(nameof(ICovariantObservableValue<>.Value));
}

public readonly struct ObservableValueChangedEventArgs<T>(T oldValue, T newValue)
{
    public T OldValue => oldValue;
    
    public T NewValue => newValue;
}

public static class ObservableValueExtensions
{
    extension<T>(IReadOnlyObservableValue<T> readOnlyObservableValue)
    {
        public IReadOnlyObservableValue<TOut> Cast<TOut>()
        {
            if (readOnlyObservableValue.Value is not TOut typedValue)
            {
                throw new InvalidCastException();
            }

            ObservableValue<TOut> output = new(typedValue);

            readOnlyObservableValue.OnChanged += (_, e) =>
            {
                if (e.NewValue is not TOut newTypedValue)
                {
                    throw new InvalidCastException();
                }

                output.Value = newTypedValue;
            };

            return output;
        }

        public IReadOnlyObservableValue<TOut> Map<TOut>(Func<T, TOut> func)
        {
            ObservableValue<TOut> output = new(func(readOnlyObservableValue.Value));
            
            readOnlyObservableValue.OnChanged += (_, e) => output.Value = func(e.NewValue);

            return output;
        }
    }
}