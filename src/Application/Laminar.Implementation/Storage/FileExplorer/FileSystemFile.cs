using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class FileSystemFile : FileSystemItem, IFileSystemFile
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileSystem _fileSystem;
    
    public FileSystemFile(
        FileSystemFolder parent,
        IPersistentDictionary persistentData,
        IFileSystem fileSystem,
        IFileSystemGraph graph,
        IServiceProvider serviceProvider)
        : base(persistentData, fileSystem, graph)
    {
        _serviceProvider = serviceProvider;
        _fileSystem = fileSystem;
        SetParent(parent);
        Info = FileSystemItemType.FromExtension(fileSystem.GetExtension(ComputePathFromParent(parent)));
        Refresh();
    }

    public long SizeOnDisk { get; private set; }

    public IFileResource<TValue> GetContentsAsResource<TValue, TData>(IPersistentDataTranscoder transcoder,
        IDecodingFactory<TValue, TData> factory)
        where TValue : class, IEncodableDataOwner<TData>, IEncodableDataOwner<IEncodableData>
        where TData : class, IEncodableData
        => ActivatorUtilities.CreateInstance<FileResource<TValue, TData>>(_serviceProvider, this, transcoder, factory);

    public override FileSystemItemType Info { get; }

    protected override void RefreshOverride()
    {
        SizeOnDisk = _fileSystem.GetFileSize(Path);
    }
}