using System.Collections.Specialized;

namespace Laminar.Domain.Notification.Collections;

public interface IReadOnlyObservableCollection<T> : IReadOnlyList<T>, INotifyCollectionChanged
{
    bool Contains(T value);
    
    int IndexOf(T value);
}
