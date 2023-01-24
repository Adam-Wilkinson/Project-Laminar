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

public class ItemAddedEventArgs<T> : EventArgs
{
    public ItemAddedEventArgs(T item)
    {
        Item = item;
    }

    public T Item { get; }
}

public class ItemRemovedEventArgs<T> : EventArgs
{
    public ItemRemovedEventArgs(T item)
    {
        Item = item;
    }

    public T Item { get; }
}