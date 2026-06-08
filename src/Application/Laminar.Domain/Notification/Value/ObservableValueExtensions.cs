namespace Laminar.Domain.Notification.Value;

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