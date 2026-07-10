using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.PersistentData;

public interface IPersistentDataManager : IDisposable
{
    public IPersistentDictionary GetDataStore(DataStoreKey dataStoreKey);
    
    public void ForgetDataStore(DataStoreKey dataStoreKey);

    public IDataOnDisk<TData> GetDataOnDisk<TData>(
        FileSystemPath filePath, 
        IPersistentDataTranscoder transcoder,
        TData? initialValue = null)
        where TData : class, IEncodableData;
}