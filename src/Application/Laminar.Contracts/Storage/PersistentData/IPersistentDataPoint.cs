namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataPoint
{
    public IPersistentValue<T> SetDefaultAndGet<T>(T defaultValue, Type? serializationKeyOverride = null, 
        object? deserializationContext = null) where T : notnull;

    public IPersistentValue<T> GetValue<T>() where T : notnull; 
    
    public object EncodedValue { get; set; }
    
    public bool IsInitialized { get; }

    public void OnDeletion();
}