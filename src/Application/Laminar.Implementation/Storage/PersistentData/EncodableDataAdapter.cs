using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

public class EncodableDataAdapter<TValue, TEncodable>(Func<TValue, TEncodable> toEncodable, Func<TEncodable, TValue> fromEncodable) : IPersistenceAdapter<TValue>
    where TEncodable : class, IEncodablePersistentData
{
    public void Persist(TValue value, IPersistentDataPoint dataPoint)
    {
        dataPoint.Reset();
        dataPoint.GetOrCreateCollection(toEncodable(value));
    }

    public void Hydrate(TValue value, IPersistentDataPoint dataPoint) => dataPoint.GetOrCreateCollection(toEncodable(value));

    public TValue Create(IPersistentDataPoint dataPoint) => fromEncodable(dataPoint.GetOrCreateCollection<TEncodable>());

    public PersistenceAdapterMode Mode => PersistenceAdapterMode.Create;
}