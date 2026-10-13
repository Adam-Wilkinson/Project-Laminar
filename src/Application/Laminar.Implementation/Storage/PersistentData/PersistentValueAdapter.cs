using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentValueAdapter<T>(Func<T?, Type>? serializationKeyFactory = null, object? deserializationContext = null) 
    : IPersistenceAdapter<T> where T : notnull
{
    public void Persist(T value, IPersistentDataPoint dataPoint)
    {
        if (!dataPoint.GetValue<T>(serializationKeyFactory?.Invoke(default), deserializationContext).TryGetResult(out var result, out var error))
        {
            throw error;
        }

        result.Value = value;
    }
    
    public void Hydrate(T value, IPersistentDataPoint dataPoint) => dataPoint.GetValueOrInitialize(value, serializationKeyFactory?.Invoke(value), deserializationContext);

    public T Create(IPersistentDataPoint dataPoint)
    {
        return !dataPoint.GetValue<T>(serializationKeyFactory?.Invoke(default), deserializationContext)
            .TryGetResult(out var result, out var error) ? throw error : result.Value;
    }

    public required PersistenceAdapterMode Mode { get; init; }
}