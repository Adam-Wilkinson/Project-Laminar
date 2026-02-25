using System.IO;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageFile : LaminarStorageItem<FileInfo>
{
    public LaminarStorageFile(FileInfo fileSystemInfo, ILaminarStorageFolder parent, IFileSystem fileSystem, ILogger<LaminarStorageItem>? logger) 
        : base(fileSystemInfo, fileSystem, logger)
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
}