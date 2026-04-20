namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentList : IPersistentDataValueOwner
{
    public T GetOrCreateChild<T>(int index) where T : notnull;

    public IObservableValueWithDefault<T> Initialize<T>(int index, T initialValue, object? deserializationContext = null);
    
    public IObservableValueWithDefault<T> Get<T>(int index);
    
    public void Set<T>(int index, T value);
    
    public void Remove(int index);
}