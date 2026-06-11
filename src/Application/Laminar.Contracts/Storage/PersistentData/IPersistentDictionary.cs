namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDictionary : IEncodablePersistentData, IReadOnlyDictionary<string, IPersistentDataPoint>
{
    public bool Remove(string key);

    public void Clear();
}