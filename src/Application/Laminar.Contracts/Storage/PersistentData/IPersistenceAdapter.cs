namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistenceAdapter<T>
{
    public void Persist(T value, IPersistentDataPoint dataPoint);
    
    public void Hydrate(T value, IPersistentDataPoint dataPoint);
    
    public T Create(IPersistentDataPoint dataPoint);
    
    public PersistenceAdapterMode Mode { get; }
}

public enum PersistenceAdapterMode
{
    /// <summary>
    /// Hydration takes existing values and updates them with the serialized value.
    /// This enables persistence in scenarios where collections are read-onl
    /// </summary>
    Hydrate,
    
    /// <summary>
    /// Creates items from the persistent data point 
    /// </summary>
    Create,
}