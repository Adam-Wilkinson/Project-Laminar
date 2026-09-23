using System.Collections;
using System.Collections.Specialized;

namespace Laminar.Domain.Observables.Collections;

public abstract class ReadOnlyObservableListBase<T> : IReadOnlyObservableList<T>, IList
{
    public abstract bool Contains(T value);
    
    public abstract int IndexOf(T value);

    public abstract IEnumerator<T> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public abstract int Count { get; }

    public abstract T this[int index] { get; }
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public event EventHandler<T>? ItemAdded;
    
    public event EventHandler<T>? ItemRemoved;
    
    protected void InvokeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(sender, e);
        if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace && ItemRemoved is not null)
        {
            foreach (var item in e.OldItems!.Cast<T>())
            {
                ItemRemoved.Invoke(sender, item);
            }
        }

        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace && ItemAdded is not null)
        {
            foreach (var item in e.NewItems!.Cast<T>())
            {
                ItemAdded.Invoke(sender, item);
            }
        }
    }
    
    object? IList.this[int index]
    {
        get => this[index];
        set => throw new InvalidOperationException("This list is read only");
    }

    int IList.Add(object? value)
    {
        throw new InvalidOperationException("This list is read only");
    }

    bool IList.Contains(object? value) => value is T typedValue && Contains(typedValue);

    void IList.Clear()
    {
        throw new InvalidOperationException("This list is read only");
    }

    bool IList.IsReadOnly => true;

    bool IList.IsFixedSize => false;

    int IList.IndexOf(object? value) => value is T typedValue ? IndexOf(typedValue) : -1;

    void IList.Insert(int index, object? value)
    {
        throw new InvalidOperationException("This list is read only");
    }

    void IList.Remove(object? value)
    {
        throw new InvalidOperationException("This list is read only");
    }

    void IList.RemoveAt(int index)
    {
        throw new InvalidOperationException("This list is read only");
    }

    void ICollection.CopyTo(Array array, int index)
    {
        throw new InvalidOperationException("This list is read only");
    }

    int ICollection.Count => Count;

    object ICollection.SyncRoot => throw new InvalidOperationException("List synchronisation not supported");

    bool ICollection.IsSynchronized => false;
}