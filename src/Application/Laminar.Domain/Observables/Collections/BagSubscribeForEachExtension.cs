namespace Laminar.Domain.Observables.Collections;

public static class BagSubscribeForEachExtensions
{
    public static IDisposable SubscribeForEach<T>(this IReadOnlyObservableBag<T> observableBag,
        Action<T>? onAdded = null, Action<T>? onRemoved = null) => new Subscription<T>(observableBag, onAdded, onRemoved);

    private class Subscription<T> : IDisposable
    {
        private readonly IReadOnlyObservableBag<T> _observableBag;
        private readonly Action<T>? _onAdded;
        private readonly Action<T>? _onRemoved;
        
        public Subscription(IReadOnlyObservableBag<T> bag, Action<T>? onAdded, Action<T>? onRemoved)
        {
            _observableBag = bag;
            _onAdded = onAdded;
            _onRemoved = onRemoved;

            if (_onAdded is not null)
            {
                bag.ItemAdded += BagOnItemAdded;
            }

            if (_onRemoved is not null)
            {
                bag.ItemRemoved += BagOnItemRemoved;
            }
        }

        private void BagOnItemRemoved(object? sender, T e)
        {
            _onRemoved!.Invoke(e);
        }

        private void BagOnItemAdded(object? sender, T e)
        {
            _onAdded!.Invoke(e);
        }

        public void Dispose()
        {
            _observableBag.ItemAdded -= BagOnItemAdded;
            _observableBag.ItemRemoved -= BagOnItemRemoved;
        }
    }
}