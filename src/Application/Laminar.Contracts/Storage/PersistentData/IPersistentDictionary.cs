namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDictionary : IPersistentDataValueOwner
{
    public IPersistentValue<T> InitializeValue<T>(string key, T defaultValue
        , object? deserializationContext = null, Type? serializationKeyOverride = null) where T : notnull;

    public IPersistentValue<T>? TryGetValue<T>(string key) where T : notnull;
    
    public bool SetValue<T>(string key, T value) where T : notnull;
    
    public bool RemoveValue(string key);
}