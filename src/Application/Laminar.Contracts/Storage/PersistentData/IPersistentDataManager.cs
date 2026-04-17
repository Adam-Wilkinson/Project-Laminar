using Laminar.Domain.DataManagement;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataManager
{
    public IPersistentDataNode GetDataStore(DataStoreKey dataStoreKey);

    public IPersistentDataNode GetHeadlessNode();
}