using System;
using System.Collections.Generic;
using System.IO;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Extensions;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageItemFactory(IFileSystem fileSystem, ILogger<LaminarStorageItem>? logger) : ILaminarStorageItemFactory
{
    private readonly Dictionary<string, ILaminarStorageItem> _allStorageItems = [];
    
    public ILaminarStorageItem FromFileSystemInfo(FileSystemInfo fileSystemInfo, ILaminarStorageFolder parent)
    {
        if (_allStorageItems.TryGetValue(fileSystemInfo.FullName, out ILaminarStorageItem? item))
        {
            return item;
        }
        
        ILaminarStorageItem newItem = fileSystemInfo switch
        {
            DirectoryInfo dir => new LaminarStorageFolder(dir, this, fileSystem, logger, parent),
            FileInfo file => new LaminarStorageFile(file, parent, fileSystem, logger),
            _ => throw new ArgumentException($"Unknown file system type {fileSystemInfo.GetType()}",
                nameof(fileSystemInfo)),
        };
        
        _allStorageItems[fileSystemInfo.FullName] = newItem;
        newItem.DependentValueChanged(x => x.Path).DependencyChanged += (_, e) =>
        {
            _allStorageItems.Remove(e.OldValue);
            _allStorageItems[e.NewValue] = newItem;
        };
        
        return newItem;
    }

    public T FromPath<T>(string path, ILaminarStorageFolder? parent = null) where T : class, ILaminarStorageItem
    {
        if (typeof(ILaminarStorageRootFolder).IsAssignableTo(typeof(T)))
        {
            if (parent is not null)
            {
                throw new ArgumentException("Root folders do not have parents");
            }
            
            return (new LaminarStorageRootFolder(new DirectoryInfo(path), this, fileSystem, logger) as T)!;
        }

        if (parent is null)
        {
            throw new ArgumentNullException(nameof(parent), "Non-root folders must be supplied with parents");
        }
        
        if (typeof(ILaminarStorageFolder).IsAssignableTo(typeof(T)))
        {
            return (FromFileSystemInfo(new DirectoryInfo(path), parent) as T)!;
        }

        if (typeof(LaminarStorageFile).IsAssignableTo(typeof(T)))
        {
            return (FromFileSystemInfo(new FileInfo(path), parent) as T)!;
        }
        
        throw new ArgumentException($"Unknown file system type {typeof(T)}", nameof(path));
    }
}