using System.Collections.Specialized;

namespace Laminar.Domain.Notification;

public interface INotifyCollectionChangedHelper
{
    public IHelperInstance<T> HelperInstance<T>(IReadOnlyObservableCollection<T> collection);

    public IHelperInstance<T> HelperInstance<T>(INotifyCollectionChanged collection);

    public interface IHelperInstance<T>
    {
        event EventHandler<ItemAddedEventArgs<T>>? ItemAdded;

        event EventHandler<ItemRemovedEventArgs<T>>? ItemRemoved;

        event EventHandler<EventArgs>? Reset;
    }
}

public class ItemAddedEventArgs<T>(T item, int index) : EventArgs
{
    public T Item { get; } = item;

    public int Index { get; } = index;
}

public class ItemRemovedEventArgs<T>(T item, int index) : EventArgs
{
    public T Item { get; } = item;

    public int Index { get; } = index;
}