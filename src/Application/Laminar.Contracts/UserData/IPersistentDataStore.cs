using System.ComponentModel;
using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.UserData;

public interface IPersistentDataStore
{
    protected static readonly PropertyChangedEventArgs ValueChangedArgs = new("Value");
    
    // public string FilePath { get; }
    
    public IPersistentDataStore CreateChild(string childDataStoreName);
    
    public DataReadResult<object?> GetItem(string key, Type type);
    
    public IObservableValue<object?> GetObservable(string key);
    
    public DataSaveResult SetItem(string key, object? value, Type type);
    
    public IPersistentDataStore InitializeDefaultValue(string key, object? value, Type type, object? deserializationContext = null);
}

public static class PersistentDataStoreExtensions
{
    public static DataSaveResult SetItem<T>(this IPersistentDataStore dataStore, string key, T value) => dataStore.SetItem(key, value, typeof(T));

    public static DataReadResult<T> GetItem<T>(this IPersistentDataStore dataStore, string key)
    {
        var result = dataStore.GetItem(key, typeof(T));
        
        return new DataReadResult<T>(result.Status == DataIoStatus.Success ? (T)result.Result : default, result.Status, result.Exception);
    }

    public static IPersistentDataStore InitializeDefaultValue<T>(this IPersistentDataStore dataStore, string key, T value,
        object? deserializationContext = null)
    {
        return dataStore.InitializeDefaultValue(key, value, typeof(T), deserializationContext);
    }
}