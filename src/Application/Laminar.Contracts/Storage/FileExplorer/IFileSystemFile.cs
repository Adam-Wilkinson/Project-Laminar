using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface IFileSystemFile : IFileSystemItem
{
    public long SizeOnDisk { get; }
    
    public IFileResource<TValue> GetContentsAsResource<TValue, TData>(IPersistentDataTranscoder transcoder,
        IDecodingFactory<TValue, TData> factory)
        where TValue : class, IEncodableDataOwner<TData>, IEncodableDataOwner<IEncodableData> 
        where TData : class, IEncodableData;
}