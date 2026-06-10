namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataStore
{
    public IPersistentDictionary Root { get; }
}