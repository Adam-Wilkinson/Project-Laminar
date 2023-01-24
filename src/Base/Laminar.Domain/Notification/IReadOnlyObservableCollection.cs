using System.Collections.Specialized;

namespace Laminar.Domain.Notification;

public interface IReadOnlyObservableCollection<T> : IReadOnlyList<T>, INotifyCollectionChanged
{
}
