namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDictionary : IPersistentDataValueOwner
{
    public IObservableValueWithDefault<T> InitializeValue<T>(string key, T defaultValue
        , object? deserializationContext = null, Type? serializationKeyOverride = null) where T : notnull;

    public IObservableValueWithDefault<T>? TryGetValue<T>(string key) where T : notnull;
    
    public bool SetValue<T>(string key, T value) where T : notnull;
    
    public bool RemoveValue(string key);
}