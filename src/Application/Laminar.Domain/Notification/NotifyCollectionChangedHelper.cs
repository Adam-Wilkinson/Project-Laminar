using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Laminar.Domain.Notification;

/// <summary>
/// Helper class to wrap an INotifyCollectionChanged for listeners who just want to know when items are added or removed
/// </summary>
public static class NotifyCollectionChangedHelper
{
    private static readonly Dictionary<INotifyCollectionChanged, object> ExistingListeners = new();
    
    public static Instance<T> HelperInstance<T>(this IReadOnlyObservableCollection<T> collection) => ((INotifyCollectionChanged)collection).HelperInstance<T>();

    public static Instance<T> HelperInstance<T>(this ObservableCollection<T> collection) => ((INotifyCollectionChanged)collection).HelperInstance<T>();
    
    public static Instance<T> HelperInstance<T>(this INotifyCollectionChanged core)
    {
        if (ExistingListeners.TryGetValue(core, out var value)) return (Instance<T>)value;
        
        value = new Instance<T>(core);
        ExistingListeners.Add(core, value);
        return (Instance<T>)value;
    }
}

public class Instance<T> : INotifyCollectionChangedHelper.IHelperInstance<T>
{
    private readonly INotifyCollectionChanged _core;
    
    public Instance(INotifyCollectionChanged core)
    {
        _core = core;
        _core.CollectionChanged += CoreCollectionChanged;
    }
    
    public event EventHandler<ItemAddedEventArgs<T>>? ItemAdded;

    public event EventHandler<ItemRemovedEventArgs<T>>? ItemRemoved;

    public event EventHandler<EventArgs>? Reset;

    private void CoreCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                ItemsAdded(e.NewItems!, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                ItemsRemoved(e.OldItems!, e.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Reset:
                Reset?.Invoke(_core, EventArgs.Empty);
                break;
            case NotifyCollectionChangedAction.Replace:
                ItemsRemoved(e.OldItems!, e.OldStartingIndex);
                ItemsAdded(e.NewItems!, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Move:
            default:
                break;
        }
    }

    private void ItemsAdded(IList items, int startIndex)
    {
        int offset = 0;
        foreach (var obj in items)
        {
            if (obj is T typedItem)
            {
                ItemAdded?.Invoke(_core, new ItemAddedEventArgs<T>(typedItem, startIndex + offset));
                offset++;
            }
        }
    }

    private void ItemsRemoved(IList items, int startingIndex)
    {
        int offset = 0;
        foreach (var obj in items)
        {
            if (obj is T typedItem)
            {
                ItemRemoved?.Invoke(_core, new ItemRemovedEventArgs<T>(typedItem, startingIndex + offset)); 
                offset++;
            }
        }
    }
}
