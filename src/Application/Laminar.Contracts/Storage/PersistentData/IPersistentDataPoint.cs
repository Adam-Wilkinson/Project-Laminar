using System.Diagnostics.CodeAnalysis;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataPoint
{
    public IPersistentValue<T> Initialize<T>(T defaultValue, Type? typeSerializationKey = null, 
        object? deserializationContext = null) where T : notnull;

    public IPersistentValue<T> GetValue<T>(); 
    
    public object EncodedValue { get; set; }
    
    public bool IsInitialized { get; }

    public void OnDeletion();
}