using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface IFileResourceFactory
{
    public MayError<IFileResource<TValue>> TryGetResource<TValue, TData>(
        IFileSystemFile file,
        IPersistentDataTranscoder transcoder,
        IDecodingFactory<TValue, TData> factory)
        where TValue : class, IEncodableDataOwner<IEncodableData>, IEncodableDataOwner<TData>
        where TData : class, IEncodableData;
}