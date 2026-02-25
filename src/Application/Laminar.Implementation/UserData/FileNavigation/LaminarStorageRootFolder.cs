using System.IO;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageRootFolder : LaminarStorageFolder, ILaminarStorageRootFolder
{
    private readonly IFileWatcher _folderWatcher;
    
    public LaminarStorageRootFolder(
        DirectoryInfo directoryInfo, 
        ILaminarStorageItemFactory factory,
        IFileSystem fileSystem,
        ILogger<LaminarStorageItem>? logger) : base(directoryInfo, factory, fileSystem, logger)
    {
        _folderWatcher = fileSystem.CreateFileWatcher(FileSystemInfo.FullName);
        _folderWatcher.IncludeSubdirectories = true;

        _folderWatcher.Renamed += ChildItem_Renamed;
        _folderWatcher.Created += ChildItem_ContentsChanged;
        _folderWatcher.Deleted += ChildItem_ContentsChanged;
    }

    private void ChildItem_ContentsChanged(object sender, FileSystemEventArgs e)
    {
        if (FindChildFromPath(e.FullPath) is not LaminarStorageItem changedChild) return;
        changedChild.ParentFolder?.Refresh();
    }

    private void ChildItem_Renamed(object sender, RenamedEventArgs e)
    {
        if (FindChildFromPath(e.OldFullPath) is not LaminarStorageItem renamedChild || e.Name is not { } newName) return;
        renamedChild.Name = newName;
    }

    private ILaminarStorageItem? FindChildFromPath(string absolutePath)
    {
        string relativePath = System.IO.Path.GetRelativePath(Path, absolutePath); 
        var childNames = relativePath.Split('/');
        int indexInChildNames = 0;
        ILaminarStorageFolder currentFolder = this;
        while (currentFolder.Path != absolutePath)
        {
            switch (FindDirectChildNamed(currentFolder, childNames[indexInChildNames]))
            {
                case { } item when item.Path == absolutePath:
                    return item;
                case ILaminarStorageFolder childFolder:
                    indexInChildNames++;
                    currentFolder = childFolder;
                    break;
                default:
                    continue;
            }
        }

        return null;
    }

    private ILaminarStorageItem? FindDirectChildNamed(ILaminarStorageFolder folder, string itemName)
    {
        foreach (var item in folder.Contents)
        {
            if (item.Name + item.Extension == itemName)
            {
                return item;
            }
        }

        return null;
    }
}