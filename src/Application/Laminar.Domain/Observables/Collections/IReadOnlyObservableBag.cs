namespace Laminar.Domain.Observables.Collections;

public interface IReadOnlyObservableBag<T> : IReadOnlyCollection<T>
{
    public event EventHandler<T>? ItemAdded;

    public event EventHandler<T>? ItemRemoved;

    public bool Contains(T value);
}