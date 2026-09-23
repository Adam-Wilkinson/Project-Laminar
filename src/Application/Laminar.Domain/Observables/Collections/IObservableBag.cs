namespace Laminar.Domain.Observables.Collections;

public interface IObservableBag<T> : ICollection<T>, IReadOnlyObservableBag<T>
{
    public new int Count { get; }

    int ICollection<T>.Count => Count;
    int IReadOnlyCollection<T>.Count => Count;
    bool ICollection<T>.IsReadOnly => false;
}