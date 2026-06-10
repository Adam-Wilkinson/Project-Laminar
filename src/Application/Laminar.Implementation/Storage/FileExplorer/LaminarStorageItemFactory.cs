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
    private const string IsFolder = "Is Folder";
    private readonly Dictionary<FileSystemPath, LaminarStorageItem> _allStorageItems = [];
    
    public ILaminarStorageItem FromPersistentData(IPersistentDictionary persistentDictionary, ILaminarStorageFolder parent)
    {
        if (parent is not LaminarStorageFolder internalParent) throw new InvalidOperationException("Parent is not LaminarStorageFolder");
        
        string name = persistentDictionary[LaminarStorageItem.NameKey].GetValue<string>().Value;
        FileSystemPath newItemPath = parent.Path.ChildPath(name);

        if (_allStorageItems.TryGetValue(newItemPath, out var existing))
        {
            return ReferenceEquals(existing.PersistentStorage, persistentDictionary) 
                ? existing 
                : throw new InvalidOperationException("Attempt to deserialize existing storage item");
        }

        if (deletedItemCache.TryFind(newItemPath) is LaminarStorageItem recentDeletion)
        {
            LogRequestedFileMatchesRecentDeletion(logger, newItemPath);
            _allStorageItems[newItemPath] = recentDeletion;
            return recentDeletion;
        }

        var isFolder = persistentDictionary[IsFolder].GetValueOrDefault(fileSystem.IsDirectory(newItemPath));
        
        LaminarStorageItem newItem = isFolder.Value
            ? new LaminarStorageFolder(internalParent, this, fileSystem, persistentDictionary, persistentDataManager, logger)
            : new LaminarStorageFile(internalParent, fileSystem, persistentDictionary, logger);
        
        RegisterNewItem(newItem);

        return newItem;
    }
    
    public ILaminarStorageItem CreateChild(string itemNameAndExtension, ILaminarStorageFolder parent, bool isFolder)
    {
        var newItemPath = parent.Path.ChildPath(itemNameAndExtension);
        if (_allStorageItems.TryGetValue(newItemPath, out var item))
        {
            return item;
        }

        IPersistentDictionary persistentData = persistentDataManager.GetHeadless<IPersistentDictionary>();
        persistentData[LaminarStorageItem.NameKey].GetValueOrDefault(itemNameAndExtension);
        persistentData[IsFolder].GetValueOrDefault(isFolder);
        return FromPersistentData(persistentData, parent);
    }

    public ILaminarStorageItem? TryGetExisting(FileSystemPath path) => _allStorageItems.GetValueOrDefault(path);

    public ILaminarStorageRootFolder CreateRootFolder(FileSystemPath path)
    {
        var newRootFolder = ActivatorUtilities.CreateInstance<LaminarStorageRootFolder>(provider, path);
        RegisterNewItem(newRootFolder);
        return newRootFolder;
    }

    private void RegisterNewItem(LaminarStorageItem newItem)
    {
        _allStorageItems[newItem.Path] = newItem;
        newItem.GetDependentValue(x => x.Path).OnChanged += (_, e) =>
        {
            _allStorageItems.Remove(e.OldValue);
            _allStorageItems[e.NewValue] = newItem;
        };

        newItem.RootFolderDisposed += (_, _) => _allStorageItems.Remove(newItem.Path);
    }

    [LoggerMessage(LogLevel.Trace, "A file at path '{path}' was already cached, returning cached value")]
    static partial void LogRequestedItemMatchesExistingItem(ILogger<LaminarStorageItem> logger, FileSystemPath path);

    [LoggerMessage(LogLevel.Trace, "A file that was requested at path '{path}' hash matches a recently deleted item, considering this a move operation")]
    static partial void LogRequestedFileMatchesRecentDeletion(ILogger<LaminarStorageItem> logger, FileSystemPath path);
}