using System.Collections;
using System.Collections.Specialized;

namespace Laminar.Domain.Notification;

/// <summary>
/// Helper class to wrap an INotifyCollectionChanged for listeners who just want to know when items are added or removed
/// </summary>
public class NotifyCollectionChangedHelper : INotifyCollectionChangedHelper
{
    readonly Dictionary<INotifyCollectionChanged, object> _existingListeners = new();

    public INotifyCollectionChangedHelper.IHelperInstance<T> HelperInstance<T>(IReadOnlyObservableCollection<T> collection) => HelperInstance<T>((INotifyCollectionChanged)collection);

    public INotifyCollectionChangedHelper.IHelperInstance<T> HelperInstance<T>(INotifyCollectionChanged core)
    {
        if (!_existingListeners.ContainsKey(core))
        {
            _existingListeners.Add(core, new Instance<T>(core));
        }

        return (Instance<T>)_existingListeners[core];
    }

    private class Instance<T> : INotifyCollectionChangedHelper.IHelperInstance<T>
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
                    AddItems(e.NewItems!);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e.OldItems!);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset?.Invoke(_core, EventArgs.Empty);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveItems(e.OldItems!);
                    AddItems(e.NewItems!);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
            }
        }

        public void AddItems(IList items)
        {
            foreach (object obj in items)
            {
                if (obj is T typedItem)
                {
                    ItemAdded?.Invoke(_core, new ItemAddedEventArgs<T>(typedItem));
                }
            }
        }

        public void RemoveItems(IList items)
        {
            foreach (object obj in items)
            {
                if (obj is T typedItem)
                {
                    ItemRemoved?.Invoke(_core, new ItemRemovedEventArgs<T>(typedItem));
                }
            }
        }
    }
}
