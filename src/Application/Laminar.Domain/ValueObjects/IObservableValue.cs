using System.ComponentModel;

namespace Laminar.Domain.ValueObjects;

public interface IObservableValue<T> : ICovariantObservableValue<T>
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
    public static readonly PropertyChangedEventArgs ValueChangedEventArgs = new(nameof(IObservableValue<>.Value));
}

public readonly struct ObservableValueChangedEventArgs<T>(T oldValue, T newValue)
{
    public T OldValue => oldValue;
    
    public T NewValue => newValue;
}

public static class ObservableValueExtensions
{
    extension<T>(IObservableValue<T> observableValue)
    {
        public IObservableValue<TOut> Cast<TOut>()
        {
            if (observableValue.Value is not TOut typedValue)
            {
                throw new InvalidCastException();
            }

            ObservableValue<TOut> output = new(typedValue);

            observableValue.OnChanged += (_, e) =>
            {
                if (e.NewValue is not TOut newTypedValue)
                {
                    throw new InvalidCastException();
                }

                output.Value = newTypedValue;
            };

            return output;
        }

        public IObservableValue<TOut> Map<TOut>(Func<T, TOut> func)
        {
            ObservableValue<TOut> output = new(func(observableValue.Value));
            
            observableValue.OnChanged += (_, e) => output.Value = func(e.NewValue);

            return output;
        }
    }
}