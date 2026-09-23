using System.Collections.Specialized;

namespace Laminar.Domain.Observables.Collections;

public interface IReadOnlyObservableList<T> : IReadOnlyList<T>, IReadOnlyObservableBag<T>, INotifyCollectionChanged
{
    int IndexOf(T value);
}
