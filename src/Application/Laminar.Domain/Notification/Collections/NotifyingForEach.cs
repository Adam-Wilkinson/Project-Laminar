using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Laminar.Domain.Notification.Collections;

public static class NotifyingForEach
{
    public static IDisposable SubscribeForEach<T>(this IReadOnlyObservableCollection<T> collection,
        Action<T>? onAdded = null,
        Action<T>? onRemoved = null,
        Action? onReset = null)
        => ((INotifyCollectionChanged)collection).SubscribeForEach(onAdded, onRemoved, onReset);

    public static IDisposable SubscribeForEach<T>(this ObservableCollection<T> collection,
        Action<T>? onAdded = null,
        Action<T>? onRemoved = null,
        Action? onReset = null)
        => ((INotifyCollectionChanged)collection).SubscribeForEach(onAdded, onRemoved, onReset);

    private static NotifyingForEachInstance<T> SubscribeForEach<T>(this INotifyCollectionChanged notifyCollectionChanged,
        Action<T>? onAdded,
        Action<T>? onRemoved,
        Action? onReset)
        => new(notifyCollectionChanged, onAdded, onRemoved, onReset);

    private class NotifyingForEachInstance<T> : IDisposable
    {
        private readonly INotifyCollectionChanged _notifyCollection;
        private readonly Action<T>? _onAdded;
        private readonly Action<T>? _onRemoved;
        private readonly Action? _onReset;
        
        public NotifyingForEachInstance(INotifyCollectionChanged notifyCollectionChanged, Action<T>? onAdded,
            Action<T>? onRemoved, Action? onReset)
        {
            _notifyCollection = notifyCollectionChanged;
            _onAdded = onAdded;
            _onRemoved = onRemoved;
            _onReset = onReset;
            _notifyCollection.CollectionChanged += CollectionChanged;
            if (_notifyCollection is IEnumerable<T> enumerable)
            {
                ItemsAdded(enumerable);
            }
        }

        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ItemsAdded(e.NewItems!.Cast<T>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ItemsRemoved(e.OldItems!.Cast<T>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ItemsAdded(e.NewItems!.Cast<T>());
                    ItemsRemoved(e.OldItems!.Cast<T>());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _onReset?.Invoke();
                    break;
                case NotifyCollectionChangedAction.Move:
                default:
                    break;
            }
        }

        private void ItemsAdded(IEnumerable<T> newItems)
        {
            if (_onAdded is null) return;
            foreach (var item in newItems)
            {
                _onAdded?.Invoke(item);
            }
        }

        private void ItemsRemoved(IEnumerable<T> removedItems)
        {
            if (_onRemoved is null) return;
            foreach (var item in removedItems)
            {
                _onRemoved?.Invoke(item);
            }
        }

        public void Dispose()
        {
            if (_notifyCollection is IEnumerable<T> currentCollection)
            {
                ItemsRemoved(currentCollection);
            }
            
            _notifyCollection.CollectionChanged -= CollectionChanged;
        }
    }
}