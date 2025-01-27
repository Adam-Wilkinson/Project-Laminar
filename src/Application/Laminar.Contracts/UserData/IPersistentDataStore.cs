using System.ComponentModel;
using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.UserData;

public interface IPersistentDataStore
{
    protected static readonly PropertyChangedEventArgs ValueChangedArgs = new("Value");
    
    public string FilePath { get; }

    public DataReadResult<T> GetItem<T>(string key)
        where T : notnull;

    public DataReadResult<object> GetItem(string key, Type type);
    
    public IObservableValue<object> GetObservable(string key);
    
    public DataSaveResult SetItem<T>(string key, T value)
        where T : notnull;
    
    public DataSaveResult SetItem(string key, object value, Type type);

    public IPersistentDataStore InitializeDefaultValue<T>(string key, T value, object? deserializationContext = null) where T : notnull;
    
    public IPersistentDataStore InitializeDefaultValue(string key, object value, Type type, object? deserializationContext = null);
}