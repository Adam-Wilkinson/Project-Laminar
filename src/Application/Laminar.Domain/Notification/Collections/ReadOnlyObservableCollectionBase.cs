using System.Collections;
using System.Collections.Specialized;

namespace Laminar.Domain.Notification.Collections;

public abstract class ReadOnlyObservableCollectionBase<T> : IReadOnlyObservableCollection<T>, IList
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

    protected void InvokeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(sender, e);
    }
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    
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