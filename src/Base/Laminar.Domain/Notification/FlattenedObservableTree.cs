using System.Collections;
using System.Collections.Specialized;
using Laminar.Domain.Extensions;

namespace Laminar.Domain.Notification;

public class FlattenedObservableTree<T> : INotifyCollectionChanged, IReadOnlyList<T>
{
    readonly List<T> _flattenedItems = new List<T>();
    readonly Dictionary<IEnumerable, FlattenedObservableTree<T>> _subTrees = new();
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

    private void AddObjects(IEnumerable? objects, int index)
    {
        if (objects is null)
        {
            return;
        }

        int flattenedObjectCount = 0;

        foreach (object obj in objects)
        {
            if (obj is IEnumerable enumerableChild)
            {
                FlattenedObservableTree<T> subTree = new(enumerableChild, this);
                AddItemRange(subTree._flattenedItems, index + flattenedObjectCount);
                flattenedObjectCount += subTree.Count;
                _subTrees.Add(enumerableChild, subTree);
                _childNodes.Add(subTree);
            }

            if (obj is T correctType)
            {
                _childNodes.Add(correctType);
                AddItem(correctType, index + flattenedObjectCount);
                flattenedObjectCount++;
            }
        }
    }

    private void RemoveObjects(IEnumerable? objects)
    {
        if (objects is null)
        {
            return;
        }

        _subTrees.Remove(objects);
    }

    private void AddItem(T item, int index)
    {
        _flattenedItems.Insert(index, item);
        NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;
        collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        _parent?.AddObjects(item.Yield(), index + _parent.GetFlattenedIndexOf(this));
    }

    private void AddItemRange(List<T> newItems, int index)
    {
        _flattenedItems.InsertRange(index, newItems);
        NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;
        collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, index));
        _parent?.AddObjects(newItems, index + _parent.GetFlattenedIndexOf(this));
    }

    private void RemoveItem(T item)
    {
        _flattenedItems.Remove(item);
        NotifyCollectionChangedEventHandler collectionChanged = CollectionChanged;
        collectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
    }

    private void CollectionNotifier_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddObjects(e.NewItems, e.NewStartingIndex);
                break;
        }
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
