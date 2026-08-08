using System.Collections.Specialized;

namespace Laminar.Domain.Observables.Collections;

public interface IReadOnlyObservableCollection<T> : IReadOnlyList<T>, INotifyCollectionChanged
{
    bool Contains(T value);
    
    int IndexOf(T value);
}
