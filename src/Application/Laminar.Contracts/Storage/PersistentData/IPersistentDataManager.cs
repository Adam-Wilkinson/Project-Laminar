using Laminar.Domain.DataManagement;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataManager
{
    public IPersistentDictionary GetDataStore(DataStoreKey dataStoreKey);

    public T GetHeadlessNode<T>() where T : IPersistentDataValueOwner;
}