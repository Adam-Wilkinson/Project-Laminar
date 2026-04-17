using System;
using System.Collections.Generic;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Extensions;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal partial class LaminarStorageItemFactory(IServiceProvider provider, IDeletedStorageItemCache deletedItemCache, ILogger<LaminarStorageItem> logger)
    : ILaminarStorageItemFactory
{
    private readonly Dictionary<FileSystemPath, ILaminarStorageItem> _allStorageItems = [];

    public ILaminarStorageItem FromPath(FileSystemPath path, ILaminarStorageFolder parent)
    {
        if (_allStorageItems.TryGetValue(path, out ILaminarStorageItem? item))
        {
            return item;
        }

        ILaminarStorageItem newItem = string.IsNullOrWhiteSpace(path.Extension)
            ? ActivatorUtilities.CreateInstance<LaminarStorageFolder>(provider, path, parent)
            : ActivatorUtilities.CreateInstance<LaminarStorageFile>(provider, path, parent);

        if (deletedItemCache.TryFind(newItem) is { } cachedItem)
        {
            LogRequestedFileMatchesRecentDeletion(logger, path);
            _allStorageItems[path] = cachedItem;
            return cachedItem;
        }
        
        _allStorageItems[path] = newItem;
        newItem.GetDependentValue(x => x.Path).OnChanged += (_, e) =>
        {
            _allStorageItems.Remove(e.OldValue);
            _allStorageItems[e.NewValue] = newItem;
        };
        
        return newItem;
    }

    public ILaminarStorageItem? TryGetExisting(FileSystemPath path) =>  _allStorageItems.GetValueOrDefault(path);

    public ILaminarStorageRootFolder CreateRootFolder(FileSystemPath path) 
        => ActivatorUtilities.CreateInstance<LaminarStorageRootFolder>(provider, path);

    [LoggerMessage(LogLevel.Trace, "A file at path '{path}' was already cached, returning cached value")]
    static partial void LogRequestedItemMatchesExistingItem(ILogger<LaminarStorageItem> logger, FileSystemPath path);

    [LoggerMessage(LogLevel.Trace, "A file that was requested at path '{path}' hash matches a recently deleted item, considering this a move operation")]
    static partial void LogRequestedFileMatchesRecentDeletion(ILogger<LaminarStorageItem> logger, FileSystemPath path);
}