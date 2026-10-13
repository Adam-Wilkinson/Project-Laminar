using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class FileSystemFile : FileSystemItem, IFileSystemFile
{
    private readonly IFileResourceFactory _resourceFactory;
    private readonly IFileSystem _fileSystem;
    
    public FileSystemFile(
        IFileSystemFolder parent,
        IPersistentDictionary persistentData,
        IFileSystem fileSystem,
        IFileSystemGraph graph,
        IFileResourceFactory resourceFactory)
        : base(persistentData, fileSystem, graph)
    {
        _resourceFactory = resourceFactory;
        _fileSystem = fileSystem;
        SetParent(parent);
        Info = FileSystemItemType.FromExtension(fileSystem.GetExtension(ComputePathFromParent(parent)));
        Refresh();
    }

    public long SizeOnDisk { get; private set; }

    public MayError<IFileResource<TValue>> GetContentsAsResource<TValue, TData>(IPersistentDataTranscoder transcoder,
        IDecodingFactory<TValue, TData> factory)
        where TValue : class, IEncodableDataOwner<TData>, IEncodableDataOwner<IEncodableData>
        where TData : class, IEncodableData
        => _resourceFactory.TryGetResource(this, transcoder, factory);

    public override FileSystemItemType Info { get; }

    protected override void RefreshOverride()
    {
        SizeOnDisk = _fileSystem.GetFileSize(Path);
    }
}