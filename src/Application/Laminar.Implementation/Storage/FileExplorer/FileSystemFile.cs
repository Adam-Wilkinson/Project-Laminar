using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class FileSystemFile : FileSystemItem, IFileSystemFile
{
    private readonly IFileSystem _fileSystem;
    private readonly IPersistentDataManager _persistentDataManager;
    
    public FileSystemFile(
        FileSystemFolder parent,
        IPersistentDictionary persistentData,
        IFileSystem fileSystem,
        IPersistentDataManager persistentDataManager,
        IFileSystemGraph graph)
        : base(persistentData, fileSystem, graph)
    {
        _fileSystem = fileSystem;
        _persistentDataManager = persistentDataManager;
        SetParent(parent);
        Info = FileSystemItemType.FromExtension(fileSystem.GetExtension(ComputePathFromParent(parent)));
        Refresh();
    }

    public long SizeOnDisk { get; private set; }

    public IFileResource<TValue> GetContentsAsResource<TValue, TData>(IPersistentDataTranscoder transcoder,
        IDecodingFactory<TValue, TData> factory)
        where TValue : class, IEncodableDataOwner<TData>, IEncodableDataOwner<IEncodableData> 
        where TData : class, IEncodableData 
        => new FileResource<TValue, TData>(_persistentDataManager, transcoder, factory, this);

    public override FileSystemItemType Info { get; }

    protected override void RefreshOverride()
    {
        SizeOnDisk = _fileSystem.GetFileSize(Path);
    }
}