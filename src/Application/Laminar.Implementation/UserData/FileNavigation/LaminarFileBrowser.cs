using System;
using System.Collections.Generic;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Laminar.Domain.Extensions;
using Laminar.Implementation.UserData.FileNavigation.UserActions;
using static Laminar.Domain.DataManagement.DataLocations;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarFileBrowser(
    IUserActionManager actionManager,
    ILaminarStorageItemFactory factory,
    IPersistentDataManager dataManager,
    IFileSystem fileSystem)
    : ILaminarFileBrowser, IDisposable
{
    private readonly ILaminarStorageRootFolder _recyclingBin 
        = factory.CreateRootFolder(LocalDataFolder.ChildPath("Recycling Bin"));
    
    public IReadOnlyObservableCollection<ILaminarStorageRootFolder> RootFolders { get; } = dataManager
        .GetDataStore(DataStoreKey.PersistentData)
        .GetOrCreateChild("FileBrowser")
        .InitializeDefaultValue<List<FileSystemPath>>(nameof(RootFolders), [RoamingDataFolder.ChildPath("Default")])
        .ToObservableCollection()
        .ObservableMap(factory.CreateRootFolder);
    
    public IUserActionResult AddDefault<T>(ILaminarStorageFolder parentFolder, IActionScope? scope = null) 
        where T : class, ILaminarStorageItem
    {
        return actionManager.ExecuteAction(new AddDefaultStorageItemAction<T>(fileSystem, parentFolder, factory, _recyclingBin), scope);
    }

    public IUserActionResult Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex,
        IActionScope? scope)
    {
        return actionManager.ExecuteAction(new MoveStorageItemAction(itemToMove, destinationFolder, fileSystem, destinationIndex), scope);
    }

    public IUserActionResult Delete<T>(T itemToDelete, IActionScope? scope) where T : class, ILaminarStorageItem
    {
        return actionManager.ExecuteAction(new MoveStorageItemAction(itemToDelete, _recyclingBin, fileSystem), scope); 
    }

    public IUserActionResult Rename(ILaminarStorageItem itemToRename, string newName, IActionScope? scope)
    {
        return actionManager.ExecuteAction(new RenameStorageItemAction(newName, itemToRename, fileSystem), scope);
    }

    public bool OpenInSystemFileBrowser(ILaminarStorageItem item)
    {
        return item.Path is { } itemPath && fileSystem.OpenInSystemFileBrowser(itemPath);
    } 
    
    public void Dispose()
    {
        foreach (ILaminarStorageRootFolder rootFolder in RootFolders)
        {
            rootFolder.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}