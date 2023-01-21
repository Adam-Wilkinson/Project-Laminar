using System.Collections;
using System.Collections.Specialized;
using Laminar.Domain.Extensions;

namespace Laminar.Domain.Notification;

public class FlattenedObservableTree<T> : INotifyCollectionChanged, IReadOnlyList<T>
{
    readonly List<T> _flattenedItems = new List<T>();
    readonly IList<object> _childNodes = new List<object>();

    readonly FlattenedObservableTree<T>? _parent;

    public FlattenedObservableTree(IEnumerable rootList)
    {
        if (rootList is INotifyCollectionChanged collectionNotifier)
        {
            collectionNotifier.CollectionChanged += CollectionNotifier_CollectionChanged;
        }

        AddObjects(rootList, 0);
    }

    private FlattenedObservableTree(IEnumerable rootList, FlattenedObservableTree<T> parent) : this(rootList)
    {
        _parent = parent;
    }

    public T this[int index] => _flattenedItems[index];

    public int Count => _flattenedItems.Count;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public IEnumerator<T> GetEnumerator() => _flattenedItems.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _flattenedItems.GetEnumerator();

    private void AddObjects(IEnumerable objects, int index)
    {
        foreach (object obj in objects)
        {
            if (obj is IEnumerable enumerableChild)
            {
                FlattenedObservableTree<T> subTree = new(enumerableChild, this);
                _childNodes.Insert(index, subTree);
                AddItemRange(subTree._flattenedItems, GetFlattenedIndex(index));
            }

            if (obj is T correctType)
            {
                _childNodes.Insert(index, correctType);
                AddItem(correctType, GetFlattenedIndex(index));
            }

            index++;
        }
    }

    private void RemoveObjects(int startingIndex, int count)
    {
        int flattenedItemCount = 0;

        for (int i = startingIndex; i < startingIndex + count; i++)
        {
            flattenedItemCount += GetChildFlattenedItemCount(_childNodes[startingIndex]);
            _childNodes.RemoveAt(startingIndex);
        }

        RemoveItemRange(_flattenedItems.GetRange(GetFlattenedIndex(startingIndex), flattenedItemCount), GetFlattenedIndex(startingIndex));
    }

    private void AddItem(T item, int index)
    {
        _flattenedItems.Insert(index, item);
        NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;
        collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        _parent?.AddItem(item, index + _parent.GetFlattenedIndexOf(this));
    }

    private void AddItemRange(List<T> newItems, int index)
    {
        _flattenedItems.InsertRange(index, newItems);
        NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;
        collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, index));
        _parent?.AddItemRange(newItems, index + _parent.GetFlattenedIndexOf(this));
    }

    private void RemoveItemRange(List<T> removedItems, int index)
    {
        _flattenedItems.RemoveRange(index, removedItems.Count);
        NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;
        collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, index));
        _parent?.RemoveItemRange(removedItems, index + _parent.GetFlattenedIndexOf(this));
    }

    private void CollectionNotifier_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddObjects(e.NewItems!, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                RemoveObjects(e.OldStartingIndex, e.OldItems!.Count);
                break;
        }
    }

    private int GetFlattenedIndex(int index)
    {
        int totalIndex = 0;
        for (int i = 0; i < index; i++)
        {
            totalIndex += GetChildFlattenedItemCount(_childNodes[i]);
        }

        return totalIndex;
    }

    private int GetFlattenedIndexOf(object child)
    {
        int index = 0;
        int flattenedIndex = 0;

        while (_childNodes[index] != child)
        {
            flattenedIndex += GetChildFlattenedItemCount(_childNodes[index]);
            index++;
        }

        return flattenedIndex;
    }

    private int GetChildFlattenedItemCount(object child)
    {
        if (child is FlattenedObservableTree<T> subTree)
        {
            return subTree.Count;
        }

        return 1;
    }
}
