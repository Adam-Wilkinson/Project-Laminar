using System.IO;
using Laminar.Contracts.UserData.FileNavigation;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageFile : LaminarStorageItem<FileInfo>
{
    public LaminarStorageFile(string path, ILaminarStorageFolder parent, ILogger<ILaminarStorageItem>? logger) 
        : this(new FileInfo(path), parent, logger)
    {
    }

    public LaminarStorageFile(FileInfo fileSystemInfo, ILaminarStorageFolder parent, ILogger<ILaminarStorageItem>? logger) 
        : base(fileSystemInfo, logger)
    {
        if (!fileSystemInfo.Exists)
        {
            fileSystemInfo.Create();
        }

        ParentFolder = parent;
    }

    protected override void MoveTo(string newPath)
    {
        FileSystemInfo.MoveTo(newPath);
    }

    public override ILaminarStorageFolder ParentFolder { get; }

    internal void ParentEnabledChanged(bool enabled)
    {
        OnPropertyChanged(nameof(IsEffectivelyEnabled));
    }
}