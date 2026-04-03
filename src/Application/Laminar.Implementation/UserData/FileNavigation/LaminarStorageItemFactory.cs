using System;
using System.Collections.Generic;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Extensions;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public partial class LaminarStorageItemFactory(IFileSystem fileSystem, ILogger<LaminarStorageItem> logger) : ILaminarStorageItemFactory
{
    private static readonly TimeSpan DeletedItemMoveDetectionCooldown = new(0, 0, 2); 
    
    private readonly Dictionary<FileSystemPath, ILaminarStorageItem> _allStorageItems = [];
    private readonly Dictionary<int, (ILaminarStorageItem item, DateTime timestamp)> _recentlyDeletedItems = [];

    public ILaminarStorageItem FromPath(FileSystemPath path, ILaminarStorageFolder parent)
    {
        if (_allStorageItems.TryGetValue(path, out ILaminarStorageItem? item))
        {
            // LogRequestedItemMatchesExistingItem(logger, path);
            return item;
        }

        LaminarStorageItem newItem = string.IsNullOrWhiteSpace(path.Extension)
            ? new LaminarStorageFolder(path, this, logger, fileSystem, parent) 
            : new LaminarStorageFile(path, parent, fileSystem, logger);

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
        newItem.GetDependentValue(x => x.Path).OnChanged += (_, e) =>
        {
            if (e.OldValue.HasValue)
            {
                _allStorageItems.Remove(e.OldValue.Value);
            }

            if (e.NewValue.HasValue)
            {
                _allStorageItems[e.NewValue.Value] = newItem;
            } 
        };
        
        return newItem;
    }

    public ILaminarStorageRootFolder CreateRootFolder(FileSystemPath path) => 
        new LaminarStorageRootFolder(path, this, fileSystem, logger);

    private int HashDeletedItem(LaminarStorageItem item) => item switch
    {
        ILaminarStorageFolder folder => HashCode.Combine(folder.Path?.NameAndExtension, folder.SizeOnDisk.Value, folder.Contents.Count),
        _ => HashCode.Combine(item.Path?.NameAndExtension, item.SizeOnDisk.Value)
    };

    [LoggerMessage(LogLevel.Trace, "A file at path '{path}' was already cached, returning cached value")]
    static partial void LogRequestedItemMatchesExistingItem(ILogger<LaminarStorageItem> logger, FileSystemPath path);

    [LoggerMessage(LogLevel.Trace, "A file that was requested at path '{path}' hash matches a recently deleted item, considering this a move operation")]
    static partial void LogRequestedFileMatchesRecentDeletion(ILogger<LaminarStorageItem> logger, FileSystemPath path);
}