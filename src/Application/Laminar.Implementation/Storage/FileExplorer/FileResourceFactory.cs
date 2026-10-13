using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.FileExplorer;

public class FileResourceFactory(
    IPersistentDataManager dataManager, 
    IFileSystem fileSystem,
    IExceptionHandler exceptionHandler) : IFileResourceFactory
{
    public MayError<IFileResource<TValue>> TryGetResource<TValue, TData>(
        IFileSystemFile file, 
        IPersistentDataTranscoder transcoder,
        IDecodingFactory<TValue, TData> factory) 
        where TValue : class, IEncodableDataOwner<IEncodableData>, IEncodableDataOwner<TData> 
        where TData : class, IEncodableData
    {
        try
        {
            return new FileResource<TValue, TData>(file, transcoder, factory, dataManager, fileSystem,
                exceptionHandler);
        }
        catch (Exception ex)
        {
            return new MayError<IFileResource<TValue>>(ex);
        }
    }
}