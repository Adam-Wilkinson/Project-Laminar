namespace Laminar.Domain.Observables.Collections;

public interface IObservableBag<T> : ICollection<T>, IReadOnlyObservableBag<T>
{
    public new int Count { get; }

    public new bool Contains(T item);
    
    int ICollection<T>.Count => Count;
    
    int IReadOnlyCollection<T>.Count => Count;
    
    bool ICollection<T>.IsReadOnly => false;
    
    bool ICollection<T>.Contains(T item) => Contains(item);
    
    bool IReadOnlyObservableBag<T>.Contains(T item) => Contains(item);
}