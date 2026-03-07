using System.IO;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageFile : LaminarStorageItem<FileInfo>
{
    private readonly ObservableValue<long> _sizeOnDisk = new();
    
    public LaminarStorageFile(FileInfo fileSystemInfo, ILaminarStorageFolder parent, ILogger<LaminarStorageItem>? logger) 
        : base(fileSystemInfo, logger)
    {
        if (!fileSystemInfo.Exists)
        {
            fileSystemInfo.Create().Close();
        }

        SetParent(this, parent);
        Refresh();
    }

    public override IObservableValue<long> SizeOnDisk => _sizeOnDisk;

    public override void Refresh()
    {
        base.Refresh();
        _sizeOnDisk.Value = FileSystemInfo.Length;
    }
}