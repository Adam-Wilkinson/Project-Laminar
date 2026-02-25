using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Laminar.Domain.Notification;

/// <summary>
/// Used to effectively ensure ObservableCollection implements IObservableCollection, via an implicit cast
/// </summary>
/// <param name="baseCollection"></param>
/// <typeparam name="T"></typeparam>
public class ObservableCollectionImpl<T>(ObservableCollection<T> baseCollection) : IObservableCollection<T>
{
    public ObservableCollectionImpl(IEnumerable<T> baseEnumerable) : this(new ObservableCollection<T>(baseEnumerable))
    {
    }
    
    private ObservableCollection<T> BaseCollection => baseCollection;

    public bool IsReadOnly => false;

    public int Count => baseCollection.Count;

    public void Move(int oldIndex, int newIndex) => BaseCollection.Move(oldIndex, newIndex);

    public T this[int index]
    {
        get => baseCollection[index];
        set => baseCollection[index] = value;
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => baseCollection.CollectionChanged += value;
        remove => baseCollection.CollectionChanged -= value;
    }

    public IEnumerator<T> GetEnumerator() => baseCollection.GetEnumerator();

    public bool Remove(T item) => baseCollection.Remove(item);
    
    public int IndexOf(T value) => baseCollection.IndexOf(value);

    public void RemoveAt(int index) => baseCollection.RemoveAt(index);

    public void Insert(int index, T item) => baseCollection.Insert(index, item);
    
    public void Add(T item) => baseCollection.Add(item);

    public void Clear() => baseCollection.Clear();

    public bool Contains(T value) => baseCollection.Contains(value);

    public void CopyTo(T[] array, int arrayIndex) => baseCollection.CopyTo(array, arrayIndex);
    
    public static implicit operator ObservableCollectionImpl<T>(ObservableCollection<T> baseCollection) 
        => new(baseCollection);

    public static implicit operator ObservableCollection<T>(ObservableCollectionImpl<T> wrapper)
        => new(wrapper.BaseCollection);
}

public static class ObservableCollectionExtensions
{
    extension<T>(ObservableCollection<T> observableCollection)
    {
        public ObservableCollectionImpl<T> ToInterfaceImpl() => new(observableCollection);
    }
}