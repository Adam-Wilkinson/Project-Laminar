using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataNode : IPersistentDataValueOwner
{
    public IPersistentDataNode GetOrCreateChild(string childName);
    
    public IPersistentDataValueOwner? Owner { get; }
    
    public IObservableValueWithDefault<T> InitializeDefaultValue<T>(string key, T defaultValue, 
        object? deserializationContext = null, Type? serializationKeyOverride = null) where T : notnull;

    public IObservableValueWithDefault<T>? TryGetValue<T>(string key) where T : notnull;
    
    public bool SetValue<T>(string key, T value) where T : notnull;
    
    public bool RemoveValue(string key);
}