using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataManager : IDisposable
{
    public IPersistentDictionary GetDataStore(DataStoreKey dataStoreKey);

    public void ForgetDataStore(DataStoreKey dataStoreKey);

    public IResourceOnDisk<TValue> GetResourceOnDisk<TValue, TData>(FileSystemPath filePath, 
        IPersistentDataTranscoder transcoder, IDecodingFactory<TValue, TData> factory)
        where TValue : class, IEncodableDataOwner<TData>
        where TData : class, IEncodableData;
}