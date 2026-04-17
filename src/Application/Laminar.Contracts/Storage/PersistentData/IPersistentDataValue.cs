using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataValue : IObservableValueWithDefault<object>
{
    public void Initialize(object defaultValue, Type? typeSerializationKey = null, object? deserializationContext = null);
    
    public IPersistentDataValueOwner? Owner { get; }
    
    public object? EncodedValue { get; set; }
    
    public Type TypeSerializationKey { get; }
    
    public bool IsInitialized { get; }
    
    public string Name { get; init; }
}