using System.Collections;
using System.Collections.Specialized;
using FlattenedIndex = System.Int32;
using TreeIndex = System.Int32;

namespace Laminar.Domain.Notification;

public class FlattenedObservableTree<T> : IReadOnlyObservableCollection<T>
{
    readonly List<T> _flattenedItems = new();
    readonly List<object> _childNodes = new();

    readonly FlattenedObservableTree<T>? _parent;

    public FlattenedObservableTree(IEnumerable rootList)
    {
        if (rootList is INotifyCollectionChanged collectionNotifier)
        {
            collectionNotifier.CollectionChanged += CollectionNotifier_CollectionChanged;
        }

        AddChildren(0, rootList);
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

    private void AddChildren(TreeIndex index, IEnumerable objects)
    {
        foreach (object obj in objects)
        {
            if (obj is T correctType)
            {
                _childNodes.Insert(index, correctType);
                AddFlattenedItem(correctType, GetFlattenedIndex(index));
            }
            else if (obj is IEnumerable enumerableChild)
            {
                FlattenedObservableTree<T> subTree = new(enumerableChild, this);
                _childNodes.Insert(index, subTree);
                AddFlattenedItemRange(subTree._flattenedItems, GetFlattenedIndex(index));
            }

            index++;
        }
    }

    private void RemoveChildren(TreeIndex startingIndex, int count)
    {
        int flattenedItemCount = 0;

        for (int i = startingIndex; i < startingIndex + count; i++)
        {
            flattenedItemCount += GetChildFlattenedItemCount(_childNodes[startingIndex]);
            _childNodes.RemoveAt(startingIndex);
        }

        RemoveFlattenedItemRange(_flattenedItems.GetRange(GetFlattenedIndex(startingIndex), flattenedItemCount), GetFlattenedIndex(startingIndex));
    }

    private void MoveChildren(int count, TreeIndex oldStartingIndex, TreeIndex newStartingIndex)
    {
        FlattenedIndex flattenedOldStartingIndex = GetFlattenedIndex(oldStartingIndex);

        FlattenedIndex flattenedItemCount = 0;

        for (int i = oldStartingIndex; i < oldStartingIndex + count; i++)
        {
            flattenedItemCount += GetChildFlattenedItemCount(_childNodes[oldStartingIndex]);
        }

        List<T> moveItemsFlat = _flattenedItems.GetRange(flattenedOldStartingIndex, flattenedItemCount);

        List<object> removedNodes = _childNodes.GetRange(oldStartingIndex, count);
        _childNodes.RemoveRange(oldStartingIndex, count);

        FlattenedIndex flattenedNewStartingIndex = GetFlattenedIndex(newStartingIndex);

        _childNodes.InsertRange(newStartingIndex, removedNodes);

        MoveFlattenedItems(moveItemsFlat, flattenedOldStartingIndex, flattenedNewStartingIndex);
    }

    private void ReplaceChildren(IList oldItems, IList newItems, TreeIndex index)
    {
        if (index == -1)
        {
            index = _childNodes.IndexOf(oldItems[0]!);
        }

        RemoveChildren(index, oldItems.Count);
        AddChildren(index, newItems);
    }

    private void MoveFlattenedItems(List<T> items, FlattenedIndex oldStartingIndex, FlattenedIndex newStartingIndex)
    {
        _flattenedItems.RemoveRange(oldStartingIndex, items.Count);
        _flattenedItems.InsertRange(newStartingIndex, items);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, newStartingIndex, oldStartingIndex));
        if (_parent is not null)
        {
            FlattenedIndex myIndexInParent = _parent.GetFlattenedIndexOf(this);
            _parent.MoveFlattenedItems(items, myIndexInParent + oldStartingIndex, myIndexInParent + newStartingIndex);
        }
    }

    private void AddFlattenedItem(T item, FlattenedIndex index)
    {
        _flattenedItems.Insert(index, item);
        NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;
        collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        _parent?.AddFlattenedItem(item, index + _parent.GetFlattenedIndexOf(this));
    }

    private void AddFlattenedItemRange(List<T> newItems, FlattenedIndex index)
    {
        _flattenedItems.InsertRange(index, newItems);
        NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;
        collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, index));
        _parent?.AddFlattenedItemRange(newItems, index + _parent.GetFlattenedIndexOf(this));
    }

    private void RemoveFlattenedItemRange(List<T> removedItems, FlattenedIndex index)
    {
        _flattenedItems.RemoveRange(index, removedItems.Count);
        NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;

        if (_flattenedItems.Count == 0)
        {
            collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        else
        {
            collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, index));
        }

        _parent?.RemoveFlattenedItemRange(removedItems, index + _parent.GetFlattenedIndexOf(this));
    }

    private void CollectionNotifier_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddChildren(e.NewStartingIndex, e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Remove:
                RemoveChildren(e.OldStartingIndex, e.OldItems!.Count);
                break;
            case NotifyCollectionChangedAction.Reset:
                RemoveChildren(0, _childNodes.Count);
                break;
            case NotifyCollectionChangedAction.Move:
                MoveChildren(e.NewItems!.Count, e.OldStartingIndex, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
                ReplaceChildren(e.OldItems!, e.NewItems!, e.NewStartingIndex);
                break;
        }
    }

    private FlattenedIndex GetFlattenedIndex(TreeIndex index)
    {
        FlattenedIndex totalIndex = 0;
        for (int i = 0; i < index; i++)
        {
            totalIndex += GetChildFlattenedItemCount(_childNodes[i]);
        }

        return totalIndex;
    }

    private FlattenedIndex GetFlattenedIndexOf(object child)
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

    private static FlattenedIndex GetChildFlattenedItemCount(object child)
    {
        return child is FlattenedObservableTree<T> subTree ? subTree.Count : 1;
    }
}
