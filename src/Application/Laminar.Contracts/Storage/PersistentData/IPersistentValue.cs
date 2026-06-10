using Laminar.Domain.Notification.Value;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentValue<T> : IObservableValue<T>, IEncodablePersistentData
{
    public bool HasDefaultValue { get; }
    
    public T DefaultValue { get; }

    public void Reset();
}