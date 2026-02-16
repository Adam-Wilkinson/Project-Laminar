using System.ComponentModel;

namespace Laminar.Domain.ValueObjects;

public interface IObservableValue<out T> : IObservableValueBase
{
    public T Value { get; }

    public event EventHandler<T>? ValueChanged;

    public static void TransferObservable(ref IObservableValue<T>? observableValue,
        IObservableValue<T>? newObservableValue, EventHandler<T> valueChangedHandler)
    {
        if (observableValue is not null)
        {
            observableValue.ValueChanged -= valueChangedHandler;
        }
        
        observableValue = newObservableValue;

        if (observableValue is not null)
        {
            observableValue.ValueChanged += valueChangedHandler;
            valueChangedHandler.Invoke(observableValue, observableValue.Value);
        }
    }
}

public interface IObservableValueBase : INotifyPropertyChanged
{
    public static readonly PropertyChangedEventArgs ValueChangedEventArgs = new(nameof(IObservableValue<>.Value));
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

            ObservableValue<TOut> output = new() { Value = typedValue };

            observableValue.ValueChanged += (o, e) =>
            {
                if (e is not TOut newTypedValue)
                {
                    throw new InvalidCastException();
                }

                output.Value = newTypedValue;
            };

            return output;
        }

        public IObservableValue<TOut> Map<TOut>(Func<T, TOut> func)
        {
            ObservableValue<TOut> output = new() { Value = func(observableValue.Value) };
            
            observableValue.ValueChanged += (o, e) => output.Value = func(e);

            return output;
        }
    }
}