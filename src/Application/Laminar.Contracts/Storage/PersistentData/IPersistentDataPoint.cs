namespace Laminar.Contracts.Storage.PersistentData;

/// <summary>
/// Holds encoded data without a known target type, to be initialized later.
/// Encoded values are also cached at these point to avoid recomputation 
/// </summary>
public interface IPersistentDataPoint : IEncodablePersistentData
{
    public void Reset();
    
    public T GetOrCreateCollection<T>(T? knownValue = null) where T : class, IEncodablePersistentData;
    
    public IPersistentValue<T> GetValue<T>(Type? serializationKeyOverride = null, object? deserializationContext = null) where T : notnull;
    
    public IPersistentValue<T> GetValueOrDefault<T>(T defaultValue, Type? serializationKeyOverride = null, object? deserializationContext = null) where T : notnull;
}