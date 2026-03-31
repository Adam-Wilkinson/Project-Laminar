using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Extensions;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public partial class LaminarStorageItemFactory(IFileSystem fileSystem, ILogger<LaminarStorageItem> logger) : ILaminarStorageItemFactory
{
    private static readonly TimeSpan DeletedItemMoveDetectionCooldown = new(0, 0, 2); 
    
    private readonly Dictionary<string, ILaminarStorageItem> _allStorageItems = [];
    private readonly Dictionary<int, (ILaminarStorageItem item, DateTime timestamp)> _recentlyDeletedItems = [];
    
    public ILaminarStorageItem FromFileSystemInfo(FileSystemInfo fileSystemInfo, ILaminarStorageFolder parent)
    {
        return FromPath(fileSystemInfo.FullName, parent);
    }

    public ILaminarStorageItem FromPath(string path, ILaminarStorageFolder parent)
    {
        if (_allStorageItems.TryGetValue(path, out ILaminarStorageItem? item))
        {
            LogRequestedItemMatchesExistingItem(logger, path);
            return item;
        }

        LaminarStorageItem newItem = System.IO.Path.HasExtension(path)
            ? new LaminarStorageFile(path, parent, fileSystem, logger)
            : new LaminarStorageFolder(path, this, logger, fileSystem, parent);

        if (_recentlyDeletedItems.TryGetValue(HashDeletedItem(newItem), out var existingItem) 
            && DateTime.Now - existingItem.timestamp < DeletedItemMoveDetectionCooldown)
        {
            LogRequestedFileMatchesRecentDeletion(logger, path);
            _allStorageItems[path] = newItem;
            return existingItem.item;
        }
        
        newItem.OnDeleted += (_, _) =>
        {
            _allStorageItems.Remove(path);
            _recentlyDeletedItems[HashDeletedItem(newItem)] = (newItem, DateTime.Now);
        };
        
        _allStorageItems[path] = newItem;
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
            
            return (new LaminarStorageRootFolder(path, this, fileSystem, logger) as T)!;
        }

        if (parent is null)
        {
            throw new ArgumentNullException(nameof(parent), "Non-root folders must be supplied with parents");
        }
        
        return (FromPath(path, parent) as T)!;
    }

    private int HashDeletedItem(LaminarStorageItem item) => item switch
    {
        ILaminarStorageFolder folder => HashCode.Combine(folder.Name, folder.SizeOnDisk.Value, folder.Contents.Count),
        _ => HashCode.Combine(item.Name + item.Extension, item.SizeOnDisk.Value)
    };

    [LoggerMessage(LogLevel.Trace, "A file at path '{path}' was already cached, returning cached value")]
    static partial void LogRequestedItemMatchesExistingItem(ILogger<LaminarStorageItem> logger, string path);

    [LoggerMessage(LogLevel.Trace, "A file that was requested at path '{path}' hash matches a recently deleted item, considering this a move operation")]
    static partial void LogRequestedFileMatchesRecentDeletion(ILogger<LaminarStorageItem> logger, string path);
}