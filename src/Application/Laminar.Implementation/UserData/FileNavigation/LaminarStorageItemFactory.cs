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
    private readonly Dictionary<int, (ILaminarStorageItem item, DateTime timestamp)> _recentlyDeletedItems = [];
    
    public ILaminarStorageItem FromFileSystemInfo(FileSystemInfo fileSystemInfo, ILaminarStorageFolder parent)
    {
        if (_allStorageItems.TryGetValue(fileSystemInfo.FullName, out ILaminarStorageItem? item))
        {
            return item;
        }
        
        LaminarStorageItem newItem = fileSystemInfo switch
        {
            DirectoryInfo dir => new LaminarStorageFolder(dir, this, logger, parent),
            FileInfo file => new LaminarStorageFile(file, parent, logger),
            _ => throw new ArgumentException($"Unknown file system type {fileSystemInfo.GetType()}", 
                nameof(fileSystemInfo)),
        };

        if (_recentlyDeletedItems.TryGetValue(HashDeletedItem(newItem), out var existingItem) 
            && (DateTime.Now - existingItem.timestamp).Seconds < 2)
        {
            return existingItem.item;
        }
        
        newItem.OnDeleted += (_, _) =>
        {
            _recentlyDeletedItems[HashDeletedItem(newItem)] = (newItem, DateTime.Now);
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
        if (typeof(ILaminarStorageRootFolder).IsAssignableFrom(typeof(T)))
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
        
        if (typeof(ILaminarStorageFolder).IsAssignableFrom(typeof(T)))
        {
            return (FromFileSystemInfo(new DirectoryInfo(path), parent) as T)!;
        }

        if (typeof(LaminarStorageFile).IsAssignableFrom(typeof(T)))
        {
            return (FromFileSystemInfo(new FileInfo(path), parent) as T)!;
        }
        
        throw new ArgumentException($"Unknown file system type {typeof(T)}", nameof(path));
    }

    private int HashDeletedItem(LaminarStorageItem item) => item switch
    {
        ILaminarStorageFolder folder => HashCode.Combine(folder.Name, folder.SizeOnDisk.Value, folder.Contents.Count),
        _ => HashCode.Combine(item.Name + item.Extension, item.SizeOnDisk.Value)
    };
}