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

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarFileBrowser(
    IUserActionManager actionManager,
    ILaminarStorageItemFactory factory,
    IPersistentDataManager dataManager,
    IFileSystem fileSystem)
    : ILaminarFileBrowser, IDisposable
{
    public IReadOnlyObservableCollection<ILaminarStorageRootFolder> RootFolders { get; } = dataManager
        .GetDataStore(DataStoreKey.PersistentData)
        .GetOrCreateChild("FileBrowser")
        .InitializeDefaultValue<List<FileSystemPath>>(nameof(RootFolders), [ dataManager.Path.ChildPath("Default") ])
        .ToObservableCollection()
        .ObservableMap(factory.CreateRootFolder);

    public IUserActionResult AddDefault<T>(ILaminarStorageFolder parentFolder, IActionScope? scope = null) 
        where T : class, ILaminarStorageItem
    {
        return actionManager.ExecuteAction(new AddDefaultStorageItemAction<T>(fileSystem, parentFolder, factory), scope);
    }

    public IUserActionResult Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex,
        IActionScope? scope)
    {
        return actionManager.ExecuteAction(new MoveStorageItemAction(itemToMove, destinationFolder, fileSystem, destinationIndex), scope);
    }

    public IUserActionResult Delete<T>(T itemToDelete, IActionScope? scope) where T : class, ILaminarStorageItem
    {
        return actionManager.ExecuteAction(new DeleteStorageItemAction<T>(fileSystem, itemToDelete), scope);
    }

    public IUserActionResult Rename(ILaminarStorageItem itemToRename, string newName, IActionScope? scope)
    {
        return actionManager.ExecuteAction(new RenameStorageItemAction(newName, itemToRename, fileSystem), scope);
    }

    public bool OpenInSystemFileBrowser(ILaminarStorageItem item)
    {
        return fileSystem.OpenInSystemFileBrowser(item.Path);
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