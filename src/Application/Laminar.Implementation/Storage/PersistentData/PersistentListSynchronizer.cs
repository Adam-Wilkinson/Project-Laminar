using System.Collections.Specialized;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Collections;

namespace Laminar.Implementation.Storage.PersistentData;

internal sealed class PersistentListSynchronizer<T> : IDisposable
{
    private readonly INotifyCollectionChanged _notifyCollectionChanged;
    private readonly IPersistenceAdapter<T> _persistenceAdapter;
    private readonly IPersistentList _persistentList;
    
    public PersistentListSynchronizer(
        IPersistentList targetList,
        IReadOnlyObservableCollection<T> changedNotifier,
        IPersistenceAdapter<T> adapter)
    {
        _notifyCollectionChanged = changedNotifier;
        _persistenceAdapter = adapter;
        _persistentList = targetList;

        switch (adapter.Mode)
        {
            case PersistenceAdapterMode.Hydrate:    
                HydrateReadOnly(changedNotifier);
                break;
            case PersistenceAdapterMode.Create:
                if (changedNotifier is not ICollection<T> writable)
                    throw new InvalidOperationException("Creating values requires a writable input");
                HydrateWritable(writable);
                break; 
            default: 
                throw new NotSupportedException();
        }
        
        changedNotifier.CollectionChanged += SourceCollectionChanged;
    }

    private void HydrateWritable(ICollection<T> writableCollection)
    {
        writableCollection.Clear();
        foreach (var item in _persistentList)
        {
            writableCollection.Add(_persistenceAdapter.Create(item));
        }
    }

    private void HydrateReadOnly(IReadOnlyObservableCollection<T> readOnlyCollection)
    {
        int indexInSource = 0;
        foreach (var persistentRow in _persistentList)
        {
            _persistenceAdapter.Hydrate(readOnlyCollection[indexInSource], persistentRow);
            indexInSource++;
        }

        while (indexInSource < readOnlyCollection.Count)
        {
            _persistenceAdapter.Hydrate(readOnlyCollection[indexInSource], _persistentList.AddNext());
            indexInSource++;
        }
    }

    private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                int addedIndex = e.NewStartingIndex;
                foreach (var newItem in e.NewItems!.Cast<T>())
                {
                    _persistenceAdapter.Persist(newItem, _persistentList.Insert(addedIndex));
                    addedIndex++;
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                for (int i = 0; i < e.OldItems!.Count; i++)
                {
                    _persistentList.RemoveAt(e.OldStartingIndex + i);
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                // Prioritize overwrite, and then delete stragglers
                if (e.OldStartingIndex != e.NewStartingIndex) throw new InvalidOperationException();
                int replacedIndex = e.NewStartingIndex;
                foreach (var newItem in e.NewItems!.Cast<T>())
                {
                    _persistenceAdapter.Persist(newItem, _persistentList[replacedIndex]);
                    replacedIndex++;
                }

                while (replacedIndex < e.OldItems!.Count)
                {
                    _persistentList.RemoveAt(replacedIndex);
                    replacedIndex++;
                }
                break;
            case NotifyCollectionChangedAction.Move:
                _persistentList.Move(e.OldStartingIndex, e.NewStartingIndex, e.OldItems!.Count);
                break;
            case NotifyCollectionChangedAction.Reset:
                _persistentList.Clear();
                break;
        }
    }

    public void Dispose()
    {
        _notifyCollectionChanged.CollectionChanged -= SourceCollectionChanged;
    }
}