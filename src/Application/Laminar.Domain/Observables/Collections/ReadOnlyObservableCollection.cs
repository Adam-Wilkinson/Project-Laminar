using System.Collections.ObjectModel;

namespace Laminar.Domain.Observables.Collections;

public class ReadOnlyObservableCollection<T>(ObservableCollection<T> core) : ReadOnlyObservableCollectionBase<T>
{
    private ObservableCollection<T> BaseCollection => core;
    
    public static implicit operator ReadOnlyObservableCollection<T>(ObservableCollection<T> baseCollection) 
        => new(baseCollection);

    public static implicit operator ObservableCollection<T>(ReadOnlyObservableCollection<T> wrapper)
        => [.. wrapper.BaseCollection];

    public override bool Contains(T value) => core.Contains(value);

    public override int IndexOf(T value) => core.IndexOf(value);

    public override IEnumerator<T> GetEnumerator() => core.GetEnumerator();

    public override int Count => core.Count;

    public override T this[int index] => core[index];
}

public static class ReadOnlyObservableCollectionExtensions
{
    extension<T>(ObservableCollection<T> source)
    {
        public IReadOnlyObservableCollection<T> ToReadonly() => new ReadOnlyObservableCollection<T>(source);
    }
}