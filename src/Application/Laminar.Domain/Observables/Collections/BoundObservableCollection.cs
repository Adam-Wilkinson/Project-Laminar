using System.Collections.Specialized;

namespace Laminar.Domain.Observables.Collections;

public sealed class BoundObservableCollection<T> : ReadOnlyObservableCollectionBase<T>, IDisposable
{
    private IReadOnlyObservableCollection<T> _source;

    public BoundObservableCollection()
    {
        _source = new ObservableCollectionImpl<T>([]);
    }

    public BoundObservableCollection(IEnumerable<T> source)
    {
        _source = new ObservableCollectionImpl<T>(source);
    }

    public BoundObservableCollection(IReadOnlyObservableCollection<T> source)
    {
        _source = source;
        _source.CollectionChanged += CurrentBindingOnCollectionChanged;
    }

    public void BindTo(IReadOnlyObservableCollection<T> source)
    {
        _source.CollectionChanged -= CurrentBindingOnCollectionChanged;
        _source = source;
        
        InvokeCollectionChanged(this,  new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        _source.CollectionChanged += CurrentBindingOnCollectionChanged;
    }

    private void CurrentBindingOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvokeCollectionChanged(this, e);
    }

    public override bool Contains(T value) => _source.Contains(value);

    public override int IndexOf(T value) => _source.IndexOf(value);

    public override IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

    public override int Count  => _source.Count;

    public override T this[int index] => _source[index];

    public void Dispose()
    {
        _source.CollectionChanged -= CurrentBindingOnCollectionChanged;
    }
}