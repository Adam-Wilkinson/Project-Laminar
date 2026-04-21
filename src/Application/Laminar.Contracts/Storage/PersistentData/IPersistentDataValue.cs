namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataValue : IObservableValueWithDefault<object>
{
    public void Initialize(object defaultValue, Type? typeSerializationKey = null, object? deserializationContext = null);
    
    public object EncodedValue { get; set; }
    
    public Type TypeSerializationKey { get; }
    
    public bool IsInitialized { get; }

    public void OnDeletion();
}