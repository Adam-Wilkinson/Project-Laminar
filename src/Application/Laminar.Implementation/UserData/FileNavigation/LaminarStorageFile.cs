using System.IO;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageFile : LaminarStorageItem
{
    private readonly ObservableValue<long> _sizeOnDisk = new();
    private readonly IFileSystem _fileSystem;
    
    public LaminarStorageFile(string path, ILaminarStorageFolder parent, IFileSystem fileSystem, ILogger<LaminarStorageItem>? logger) 
        : base(logger)
    {
        _fileSystem = fileSystem;
        
        if (!_fileSystem.Exists(path))
        {
            _fileSystem.CreateFile(path).Close();
        }

        Extension = System.IO.Path.GetExtension(path);
        Name = System.IO.Path.GetFileNameWithoutExtension(path);
        SetParent(this, parent);
        Refresh();
    }

    public override IObservableValue<long> SizeOnDisk => _sizeOnDisk;

    public override void Refresh()
    {
        _sizeOnDisk.Value = _fileSystem.GetFileSize(Path);
    }
}