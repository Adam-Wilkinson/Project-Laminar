using System.Collections;
using System.Collections.Specialized;

namespace Laminar.Domain.Observables.Collections;

public sealed class BoundObservableList<T> : ReadOnlyObservableListBase<T>, IDisposable
{
    private IReadOnlyObservableList<T>? _source;

    public BoundObservableList()
    {
    }
    
    public BoundObservableList(IReadOnlyObservableList<T> source)
    {
        _source = source;
        _source.CollectionChanged += CurrentBindingOnCollectionChanged;
    }

    public void BindTo(IReadOnlyObservableList<T>? source)
    {
        _source?.CollectionChanged -= CurrentBindingOnCollectionChanged;
        var oldSource = _source;
        _source = source;

        switch (GetAsList(oldSource), GetAsList(source))
        {
            case ({ } oldItems, { } newItems):
                InvokeCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, 0));
                break;
            case (null, { } newItems):
                InvokeCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, 0));
                break;
            case ({ } oldItems, null):
                InvokeCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, 0));
                break;
            case (null, null):
                break;
        }
        
        InvokeCollectionChanged(this,  new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        _source?.CollectionChanged += CurrentBindingOnCollectionChanged;
    }

    private void CurrentBindingOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvokeCollectionChanged(this, e);
    }

    public override bool Contains(T value) => _source?.Contains(value) ?? false;

    public override int IndexOf(T value) => _source?.IndexOf(value) ?? -1;

    public override IEnumerator<T> GetEnumerator() => _source?.GetEnumerator() ?? Enumerable.Empty<T>().GetEnumerator();

    public override int Count  => _source?.Count ?? 0;

    public override T this[int index] => _source is not null ? _source[index] : throw new IndexOutOfRangeException();

    public void Dispose()
    {
        _source?.CollectionChanged -= CurrentBindingOnCollectionChanged;
    }

    private static IList? GetAsList(INotifyCollectionChanged? source) => source switch
    {
        IList direct => direct,
        IEnumerable<T> enumerable => enumerable.ToList(),
        null => null,
        not null => throw new InvalidOperationException($"Unable to get list from type {source.GetType()}")
    };
}