using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface ILaminarStorageFile : ILaminarStorageItem
{
    public long SizeOnDisk { get; }
    
    public ILaminarFileResource<TValue> GetContentsAsResource<TValue, TData>(IPersistentDataTranscoder transcoder,
        IDecodingFactory<TValue, TData> factory)
        where TValue : class, IEncodableDataOwner<TData>, IEncodableDataOwner<IEncodableData> 
        where TData : class, IEncodableData;
}