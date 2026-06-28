using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataManager : IDisposable
{
    public IPersistentDictionary GetDataStore(DataStoreKey dataStoreKey);

    public void ForgetDataStore(DataStoreKey dataStoreKey);

    IFileSyncedResource<T> GetFileSyncedResource<T>(T value, IPersistentDataTranscoder transcoder, FileSystemPath filePath)
        where T : class, IEncodableDataOwner;
}