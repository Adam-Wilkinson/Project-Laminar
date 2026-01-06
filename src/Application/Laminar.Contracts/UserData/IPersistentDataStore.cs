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
    
    public DataSaveResult SetItem(string key, object? value);
    
    public IPersistentDataStore InitializeDefaultValue(string key, object? value, Type type, object? deserializationContext = null);
    
    public DataReadResult<object?> GetDefaultValue(string key);
}

public static class PersistentDataStoreExtensions
{
    extension(IPersistentDataStore dataStore)
    {
        public DataSaveResult SetItem<T>(string key, T value) => dataStore.SetItem(key, value);

        public DataReadResult<T> GetItem<T>(string key)
        {
            var result = dataStore.GetItem(key, typeof(T));
        
            return new DataReadResult<T>(result.Status == DataIoStatus.Success ? (T)result.Result : default, result.Status, result.Exception);
        }

        public IPersistentDataStore InitializeDefaultValue<T>(string key, T value,
            object? deserializationContext = null)
        {
            return dataStore.InitializeDefaultValue(key, value, typeof(T), deserializationContext);
        }

        public void ResetToDefault(string key) => dataStore.SetItem(key, dataStore.GetDefaultValue(key).Result);
    }
}