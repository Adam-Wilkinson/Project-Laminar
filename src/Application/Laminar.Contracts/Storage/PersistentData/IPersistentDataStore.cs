namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataStore : IPersistentDataValueOwner
{
    public IPersistentDictionary Root { get; }
}