namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentList : IPersistentDataValueOwner
{
    public int Count { get; }
    
    public IPersistentValue<T> AddAndInitialize<T>(T initialValue, object? deserializationContext = null,
        Type? serializationKeyOverride = null) where T : notnull;

    public IPersistentValue<T> InsertAndInitialize<T>(int index, T initialValue,
        object? deserializationContext = null, Type? serializationKeyOverride = null) where T : notnull;
    
    public IPersistentValue<T> GetValue<T>(int index);
    
    public void SetValue<T>(int index, T value) where T : notnull;
    
    public void RemoveValue(int index);
}