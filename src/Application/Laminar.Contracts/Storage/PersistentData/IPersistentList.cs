namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentList : IPersistentDataValueOwner
{
    public int Count { get; }
    
    public IObservableValueWithDefault<T> AddAndInitialize<T>(T initialValue, object? deserializationContext = null,
        Type? serializationKeyOverride = null) where T : notnull;

    public IObservableValueWithDefault<T> InsertAndInitialize<T>(int index, T initialValue,
        object? deserializationContext = null, Type? serializationKeyOverride = null) where T : notnull;
    
    public IObservableValueWithDefault<T> GetValue<T>(int index);
    
    public void SetValue<T>(int index, T value) where T : notnull;
    
    public void RemoveValue(int index);
}