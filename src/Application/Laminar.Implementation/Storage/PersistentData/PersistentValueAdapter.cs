using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentValueAdapter<T>(Func<T, Type>? serializationKeyFactory = null, object? deserializationContext = null) 
    : IPersistenceAdapter<T> where T : notnull
{
    public void Persist(T value, IPersistentDataPoint dataPoint) => dataPoint.GetValue<T>().Value = value;
    
    public void Hydrate(T value, IPersistentDataPoint dataPoint) => dataPoint.GetValueOrDefault(value, serializationKeyFactory?.Invoke(value), deserializationContext);

    public T Create(IPersistentDataPoint dataPoint) => dataPoint.GetValue<T>().Value;

    public required PersistenceAdapterMode Mode { get; init; }
}