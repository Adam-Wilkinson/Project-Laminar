using Laminar.Domain.Notification.Collections;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentList : IEncodablePersistentData, IReadOnlyList<IPersistentDataPoint> 
{
    public IPersistentDataPoint AddNext();

    public IPersistentDataPoint Insert(int index);
    
    public void Move(int oldIndex, int newIndex, int itemCount);
    
    public IPersistentDataPoint RemoveAt(int index);
    
    public void Clear();

    public IDisposable InitializeAndSyncTo<T>(IReadOnlyObservableCollection<T> target, IPersistenceAdapter<T> adapter);
}