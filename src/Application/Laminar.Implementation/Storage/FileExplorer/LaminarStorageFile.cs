using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class LaminarStorageFile : LaminarStorageItem, ILaminarStorageFile
{
    private readonly IFileSystem _fileSystem;
    private readonly IPersistentDataManager _persistentDataManager;
    
    public LaminarStorageFile(
        LaminarStorageFolder parent,
        IPersistentDictionary persistentData,
        IFileSystem fileSystem,
        IPersistentDataManager persistentDataManager,
        ILogger<LaminarStorageItem> logger)
        : base(fileSystem, logger, persistentData)
    {
        _fileSystem = fileSystem;
        _persistentDataManager = persistentDataManager;
        FileSystemPath path = parent.Path.ChildPath(persistentData[NameKey].GetValue<string>().Value);
        Info = StorageItemType.FromExtension(fileSystem.GetExtension(path));

        if (!_fileSystem.Exists(path))
        {
            _fileSystem.CreateFile(path).Close();
        }
        
        SetParent(parent);
        Refresh();
    }

    public long SizeOnDisk { get; private set; }

    public ILaminarFileResource<TValue> GetContentsAsResource<TValue, TData>(IPersistentDataTranscoder transcoder,
        IDecodingFactory<TValue, TData> factory)
        where TValue : class, IEncodableDataOwner<TData>, IEncodableDataOwner<IEncodableData> 
        where TData : class, IEncodableData 
        => new LaminarFileResource<TValue, TData>(_persistentDataManager, transcoder, factory, this);

    public override StorageItemType Info { get; }

    protected override void RefreshOverride()
    {
        SizeOnDisk = _fileSystem.GetFileSize(Path);
    }
}