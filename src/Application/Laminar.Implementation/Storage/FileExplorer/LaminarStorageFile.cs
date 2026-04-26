using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class LaminarStorageFile : LaminarStorageItem, ILaminarStorageFile
{
    private readonly IFileSystem _fileSystem;

    public LaminarStorageFile(
        LaminarStorageFolder parent,
        IFileSystem fileSystem,
        IPersistentDictionary persistentData,
        ILogger<LaminarStorageItem> logger)
        : base(fileSystem, logger, persistentData)
    {
        _fileSystem = fileSystem;
        FileSystemPath path = parent.Path.ChildPath(persistentData[NameKey].GetValue<string>().Value);

        if (!_fileSystem.Exists(path))
        {
            _fileSystem.CreateFile(path).Close();
        }
        
        SetParent(parent);
        Refresh();
    }

    public long SizeOnDisk { get; private set; }

    protected override void RefreshOverride()
    {
        SizeOnDisk = _fileSystem.GetFileSize(Path);
    }
}