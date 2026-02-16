using System.IO;
using Laminar.Contracts.UserData.FileNavigation;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageRootFolder : LaminarStorageFolder, ILaminarStorageRootFolder
{
    public LaminarStorageRootFolder(
        string path, 
        ILaminarStorageItemFactory factory, 
        ILogger<ILaminarStorageItem>? logger) : base(path, factory, logger)
    {
    }

    public LaminarStorageRootFolder(
        DirectoryInfo directoryInfo, 
        ILaminarStorageItemFactory factory, 
        ILogger<ILaminarStorageItem>? logger) : base(directoryInfo, factory, logger)
    {
    }
}