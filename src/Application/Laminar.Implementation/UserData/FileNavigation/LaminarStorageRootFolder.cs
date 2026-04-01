using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageRootFolder : LaminarStorageFolder, ILaminarStorageRootFolder
{
    private readonly IFileWatcher _folderWatcher;

    public LaminarStorageRootFolder(
        FileSystemPath path, 
        ILaminarStorageItemFactory factory,
        IFileSystem fileSystem,
        ILogger<LaminarStorageItem>? logger) : base(path, factory, fileSystem, logger)
    {
        Path = path;
        Refresh();
        
        _folderWatcher = fileSystem.CreateFileWatcher(path);
        _folderWatcher.IncludeSubdirectories = true;
        _folderWatcher.EnableRaisingEvents = true;

        _folderWatcher.Renamed += ChildItem_Renamed;
        _folderWatcher.Created += ChildItem_Created;
        _folderWatcher.Deleted += ChildItem_Deleted;
        _folderWatcher.Changed += ChildItem_Changed;
    }

    public override FileSystemPath Path { get; }

    private void ChildItem_Changed(object sender, FileSystemEventArgs e)
    {
        FindChildrenFromPath(e.FullPath).LastOrDefault()?.Refresh();
    }

    private void ChildItem_Deleted(object sender, FileSystemEventArgs e)
    {
        if (FindChildrenFromPath(e.FullPath).LastOrDefault() is LaminarStorageItem item)
        {
            TriggerOnDeleted(item);
        }
        
        if (new FileInfo(e.FullPath).Directory?.FullName is { } parentPath)
        {
            FindChildrenFromPath(parentPath).LastOrDefault()?.Refresh();
        }
    }

    private void ChildItem_Created(object sender, FileSystemEventArgs e)
    {
        if (new FileInfo(e.FullPath).Directory?.FullName is not { } parentPath) return;
        FindChildrenFromPath(parentPath).LastOrDefault()?.Refresh();
    }

    private void ChildItem_Renamed(object sender, RenamedEventArgs e)
    {
        if (FindChildrenFromPath(e.OldFullPath).LastOrDefault() is not LaminarStorageItem renamedChild) return;
        Rename(renamedChild, new FileSystemPath(e.OldFullPath).NameAndExtension);
    }
    
    private IEnumerable<ILaminarStorageItem> FindChildrenFromPath(string absolutePath)
    {
        string relativePath = System.IO.Path.GetRelativePath(Path.ToString(), absolutePath);
        yield return this;
        var childNames = relativePath.Split('/');
        int indexInChildNames = 0;
        ILaminarStorageFolder currentFolder = this;
        while (currentFolder.Path.ToString() != absolutePath)
        {
            switch (FindDirectChildNamed(currentFolder, childNames[indexInChildNames]))
            {
                case { } item when item.Path.ToString() == absolutePath:
                    yield return item;
                    yield break;
                case ILaminarStorageFolder childFolder:
                    indexInChildNames++;
                    currentFolder = childFolder;
                    yield return currentFolder;
                    break;
                default:
                    continue;
            }
        }
    }

    private ILaminarStorageItem? FindDirectChildNamed(ILaminarStorageFolder folder, string itemName)
    {
        Span<ILaminarStorageItem> contents = folder.Contents.ToArray();
        foreach (var item in contents)
        {
            if (item.Path.NameAndExtension == itemName)
            {
                return item;
            }
        }

        return null;
    }

    public void Dispose()
    {
        _folderWatcher.Dispose();
        GC.SuppressFinalize(this);
    }
}