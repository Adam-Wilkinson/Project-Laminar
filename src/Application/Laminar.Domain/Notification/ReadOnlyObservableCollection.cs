using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Laminar.Domain.Notification;

public class ReadOnlyObservableCollection<T>(ObservableCollection<T> core) : IReadOnlyObservableCollection<T>
{
    private ObservableCollection<T> BaseCollection => core;
    
    public IEnumerator<T> GetEnumerator() => core.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => core.Count;

    public T this[int index] => core[index];

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => core.CollectionChanged += value;
        remove => core.CollectionChanged -= value;
    }
    
    public bool Contains(T value) => core.Contains(value);

    public int IndexOf(T value) => core.IndexOf(value);
    
    public static implicit operator ReadOnlyObservableCollection<T>(ObservableCollection<T> baseCollection) 
        => new(baseCollection);

    public static implicit operator ObservableCollection<T>(ReadOnlyObservableCollection<T> wrapper)
        => new(wrapper.BaseCollection);
}

public static class ReadOnlyObservableCollectionExtensions
{
    extension<T>(ObservableCollection<T> source)
    {
        public IReadOnlyObservableCollection<T> ToReadonly() => new ReadOnlyObservableCollection<T>(source);
    }
}