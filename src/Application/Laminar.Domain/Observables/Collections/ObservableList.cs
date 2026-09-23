using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Laminar.Domain.Observables.Collections;

public class ObservableList<T> : IObservableList<T>
{
    private readonly ObservableCollection<T> _baseCollection;

    public ObservableList(List<T> items)
    {
        _baseCollection = new ObservableCollection<T>(items);
    }
    
    public ObservableList(IEnumerable<T> baseEnumerable)
    {
        _baseCollection = [.. baseEnumerable];
    }

    public ObservableList()
    {
        _baseCollection = [];
    }
    
    public bool IsReadOnly => false;

    public int Count => _baseCollection.Count;

    public void Move(int oldIndex, int newIndex) => _baseCollection.Move(oldIndex, newIndex);

    public T this[int index]
    {
        get => _baseCollection[index];
        set => _baseCollection[index] = value;
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => _baseCollection.CollectionChanged += value;
        remove => _baseCollection.CollectionChanged -= value;
    }
    
    public event EventHandler<T>? ItemAdded;
    
    public event EventHandler<T>? ItemRemoved;

    public IEnumerator<T> GetEnumerator() => _baseCollection.GetEnumerator();

    public bool Remove(T item)
    {
        var result = _baseCollection.Remove(item);
        ItemRemoved?.Invoke(this, item);
        return result;
    }
    
    public int IndexOf(T value) => _baseCollection.IndexOf(value);

    public void RemoveAt(int index)
    {
        var item = _baseCollection[index];
        _baseCollection.RemoveAt(index);
        ItemRemoved?.Invoke(this, item);
    }

    public void Insert(int index, T item)
    {
        _baseCollection.Insert(index, item);
        ItemAdded?.Invoke(this, item);
    }
    
    public void Add(T item)
    {
        _baseCollection.Add(item);
        ItemAdded?.Invoke(this, item);
    }

    public void Clear()
    {
        var cache = _baseCollection.ToList();
        _baseCollection.Clear();
        foreach (var item in cache)
        {
            ItemRemoved?.Invoke(this, item);
        }
    }

    public bool Contains(T value) => _baseCollection.Contains(value);

    public void CopyTo(T[] array, int arrayIndex) => _baseCollection.CopyTo(array, arrayIndex);
}

public static class ObservableCollectionExtensions
{
    extension<T>(ObservableCollection<T> observableCollection)
    {
        public ObservableList<T> ToObservableList() => [.. observableCollection];
    }
}