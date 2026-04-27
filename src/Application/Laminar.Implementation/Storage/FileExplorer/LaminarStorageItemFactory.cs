using System;
using System.Collections.Generic;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Extensions;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal partial class LaminarStorageItemFactory(
    IServiceProvider provider,
    IFileSystem fileSystem,
    IPersistentDataManager persistentDataManager,
    IDeletedStorageItemCache deletedItemCache, 
    ILogger<LaminarStorageItem> logger)
    : ILaminarStorageItemFactory
{
    private readonly Dictionary<FileSystemPath, ILaminarStorageItem> _allStorageItems = [];

    public ILaminarStorageItem FromPersistentData(IPersistentDictionary persistentDictionary, ILaminarStorageFolder parent)
    {
        if (parent is not LaminarStorageFolder internalParent) throw new InvalidOperationException("Parent is not LaminarStorageFolder");
        
        string name = persistentDictionary[LaminarStorageItem.NameKey].GetValue<string>().Value;
        FileSystemPath newItemPath = parent.Path.ChildPath(name);

        if (_allStorageItems.TryGetValue(newItemPath, out _))
        {
            throw new InvalidOperationException("Attempt to deserialize existing storage item");
        }

        if (deletedItemCache.TryFind(newItemPath) is { } recentDeletion)
        {
            LogRequestedFileMatchesRecentDeletion(logger, newItemPath);
            _allStorageItems[newItemPath] = recentDeletion;
            return recentDeletion;
        }
        
        ILaminarStorageItem newItem = string.IsNullOrWhiteSpace(newItemPath.Extension)
            ? new LaminarStorageFolder(internalParent, this, fileSystem, persistentDictionary, persistentDataManager, logger)
            : new LaminarStorageFile(internalParent, fileSystem, persistentDictionary, logger);
        
        _allStorageItems[newItemPath] = newItem;
        newItem.GetDependentValue(x => x.Path).OnChanged += (_, e) =>
        {
            _allStorageItems.Remove(e.OldValue);
            _allStorageItems[e.NewValue] = newItem;
        };

        return newItem;
    }
    
    public ILaminarStorageItem FromPath(FileSystemPath path, ILaminarStorageFolder parent)
    {
        if (_allStorageItems.TryGetValue(path, out ILaminarStorageItem? item))
        {
            return item;
        }

        IPersistentDictionary persistentData = persistentDataManager.GetHeadlessNode<IPersistentDictionary>();
        persistentData[LaminarStorageItem.NameKey].SetDefaultAndGet(path.NameAndExtension);
        return FromPersistentData(persistentData, parent);
    }

    public ILaminarStorageItem? TryGetExisting(FileSystemPath path) =>  _allStorageItems.GetValueOrDefault(path);

    public ILaminarStorageRootFolder CreateRootFolder(FileSystemPath path) 
        => ActivatorUtilities.CreateInstance<LaminarStorageRootFolder>(provider, path);

    [LoggerMessage(LogLevel.Trace, "A file at path '{path}' was already cached, returning cached value")]
    static partial void LogRequestedItemMatchesExistingItem(ILogger<LaminarStorageItem> logger, FileSystemPath path);

    [LoggerMessage(LogLevel.Trace, "A file that was requested at path '{path}' hash matches a recently deleted item, considering this a move operation")]
    static partial void LogRequestedFileMatchesRecentDeletion(ILogger<LaminarStorageItem> logger, FileSystemPath path);
}