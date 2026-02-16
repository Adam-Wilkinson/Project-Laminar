using System;
using System.IO;
using Laminar.Contracts.UserData.FileNavigation;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageItemFactory(ILogger<ILaminarStorageItem>? logger) : ILaminarStorageItemFactory
{
    public ILaminarStorageItem FromPath(string path, ILaminarStorageFolder? folder)
        => (System.IO.File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory
            ? new LaminarStorageFolder(path, this, logger, folder)
            : new LaminarStorageFile(path, folder ?? new LaminarStorageFolder(new DirectoryInfo(path).Parent!, this, logger), logger);

    public ILaminarStorageItem FromFileSystemInfo(FileSystemInfo fileSystemInfo, ILaminarStorageFolder? parent = null) => fileSystemInfo switch
    {
        DirectoryInfo dir => new LaminarStorageFolder(dir, this, logger, parent),
        FileInfo file => new LaminarStorageFile(file, parent ?? new LaminarStorageFolder(file.Directory!, this, logger), logger),
        _ => throw new ArgumentException($"Unknown file system type {fileSystemInfo.GetType()}", nameof(fileSystemInfo)),
    };

    public T FromPath<T>(string path, ILaminarStorageFolder? parent = null) where T : class, ILaminarStorageItem
    {
        if (typeof(ILaminarStorageRootFolder).IsAssignableTo(typeof(T)))
        {
            if (parent is not null)
            {
                throw new ArgumentException("Root folders do not have parents");
            }
            
            return (new LaminarStorageRootFolder(path, this, logger) as T)!;
        }
        
        parent ??= new LaminarStorageFolder(new DirectoryInfo(path).Parent!, this, logger); 
        
        if (typeof(ILaminarStorageFolder).IsAssignableTo(typeof(T)))
        {
            return (new LaminarStorageFolder(path, this, logger, parent) as T)!;
        }

        if (typeof(LaminarStorageFile).IsAssignableTo(typeof(T)))
        {
            return (new LaminarStorageFile(path, parent, logger) as T)!;
        }
        
        throw new ArgumentException($"Unknown file system type {typeof(T)}", nameof(path));
    }

    public T AddDefaultToFolder<T>(ILaminarStorageFolder folder) where T : class, ILaminarStorageItem
    {
        object newItem = typeof(T) switch
        {
            var type when type == typeof(LaminarStorageFolder) || type == typeof(ILaminarStorageFolder) =>
                new LaminarStorageFolder(Path.Join(folder.Path, "Untitled Folder"), this, logger, folder),
            var type when type == typeof(LaminarStorageFile) => new LaminarStorageFile(
                Path.Join(folder.Path, "Untitled Script.pls"), folder, logger),
            _ => throw new ArgumentException($"Unknown file system type {typeof(T)}")
        };

        if (newItem is not T typedItem) throw new Exception();

        typedItem.NeedsName = true;
        folder.Contents.Add(typedItem);
        return typedItem;
    }
}