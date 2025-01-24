using System;
using System.IO;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageItemFactory : ILaminarStorageItemFactory
{
    public ILaminarStorageItem FromPath(string path, ILaminarStorageFolder? folder)
        => (System.IO.File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory
            ? new LaminarStorageFolder(path, this, folder)
            : new LaminarStorageFile(path, folder ?? new LaminarStorageFolder(new DirectoryInfo(path).Parent!, this));

    public ILaminarStorageItem FromFileSystemInfo(FileSystemInfo fileSystemInfo, ILaminarStorageFolder? parent = null) => fileSystemInfo switch
    {
        DirectoryInfo dir => new LaminarStorageFolder(dir, this, parent),
        FileInfo file => new LaminarStorageFile(file, parent ?? new LaminarStorageFolder(file.Directory!, this)),
        _ => throw new ArgumentException($"Unknown file system type {fileSystemInfo.GetType()}", nameof(fileSystemInfo)),
    };

    public T FromPath<T>(string path, ILaminarStorageFolder? parent = null) where T : class, ILaminarStorageItem
    {
        parent ??= new LaminarStorageFolder(new DirectoryInfo(path).Parent!, this); 
        
        if (typeof(ILaminarStorageFolder).IsAssignableFrom(typeof(T)))
        {
            return (new LaminarStorageFolder(path, this, parent) as T)!;
        }

        if (typeof(LaminarStorageFile).IsAssignableFrom(typeof(T)))
        {
            return (new LaminarStorageFile(path, parent) as T)!;
        }
        
        throw new ArgumentException($"Unknown file system type {typeof(T)}", nameof(path));
    }

    public T AddDefaultToFolder<T>(ILaminarStorageFolder folder) where T : class, ILaminarStorageItem
    {
        object newItem = typeof(T) switch
        {
            var type when type == typeof(LaminarStorageFolder) || type == typeof(ILaminarStorageFolder) =>
                new LaminarStorageFolder(Path.Join(folder.Path, "Untitled Folder"), this, folder),
            var type when type == typeof(LaminarStorageFile) => new LaminarStorageFile(
                Path.Join(folder.Path, "Untitled Script.pls"), folder),
            _ => throw new ArgumentException($"Unknown file system type {typeof(T)}")
        };

        if (newItem is not T typedItem) throw new Exception();

        typedItem.NeedsName = true;
        folder.Contents.Add(typedItem);
        return typedItem;
    }
}