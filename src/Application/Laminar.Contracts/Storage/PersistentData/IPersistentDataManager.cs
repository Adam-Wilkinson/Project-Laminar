using Laminar.Domain.DataManagement;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataManager : IDisposable
{
    public IPersistentDictionary GetDataStore(DataStoreKey dataStoreKey);

    public void ForgetDataStore(DataStoreKey dataStoreKey);

    public T GetHeadless<T>() where T : IEncodablePersistentData;
}