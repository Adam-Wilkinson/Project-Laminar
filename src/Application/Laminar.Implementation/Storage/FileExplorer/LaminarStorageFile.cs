using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class LaminarStorageFile : LaminarStorageItem, ILaminarStorageFile
{
    private readonly IFileSystem _fileSystem;
    
    public LaminarStorageFile(
        FileSystemPath path, 
        ILaminarStorageFolder parent, 
        IFileSystem fileSystem,
        ILogger<LaminarStorageItem> logger) 
        : base(fileSystem, logger)
    {
        _fileSystem = fileSystem;
        
        if (!_fileSystem.Exists(path))
        {
            _fileSystem.CreateFile(path).Close();
        }

        SetParent(parent);
        Rename(path.NameAndExtension);
        Refresh();
    }

    public long SizeOnDisk { get; private set; }

    protected override void RefreshOverride()
    {
        SizeOnDisk = _fileSystem.GetFileSize(Path);
    }
}