namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDictionary : IPersistentDataValueOwner, IDictionary<string, IPersistentDataPoint>
{
    public IPersistentValue<T>? TryGetValue<T>(string key) where T : notnull;
    
    public bool SetValue<T>(string key, T value) where T : notnull;
    
    public bool RemoveValue(string key);
}