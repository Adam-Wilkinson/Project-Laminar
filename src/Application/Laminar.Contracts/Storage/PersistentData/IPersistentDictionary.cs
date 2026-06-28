namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDictionary : IEncodableData, IReadOnlyDictionary<string, IPersistentDataPoint>
{
    public bool Remove(string key);

    public void Clear();
}