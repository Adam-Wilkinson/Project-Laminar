using System.IO;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageFile : LaminarStorageItem
{
    private readonly ObservableValue<long> _sizeOnDisk = new(0);
    private readonly IFileSystem _fileSystem;
    
    public LaminarStorageFile(FileSystemPath path, ILaminarStorageFolder parent, IFileSystem fileSystem, ILogger<LaminarStorageItem> logger) 
        : base(fileSystem, logger)
    {
        _fileSystem = fileSystem;
        
        if (!_fileSystem.Exists(path))
        {
            _fileSystem.CreateFile(path).Close();
        }

        SetParent(this, parent);
        Rename(path.NameAndExtension);
        Refresh();
    }

    public override IObservableValue<long> SizeOnDisk => _sizeOnDisk;

    protected override void RefreshOverride()
    {
        _sizeOnDisk.Value = _fileSystem.GetFileSize(Path);
    }
}