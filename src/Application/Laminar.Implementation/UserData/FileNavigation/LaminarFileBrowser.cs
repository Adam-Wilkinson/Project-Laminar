using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Extensions;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.UserData.FileNavigation.UserActions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    
    public Task<IUserActionResult> AddDefault<T>(ILaminarStorageFolder parentFolder) where T : class, ILaminarStorageItem
    {
        return actionManager.ExecuteAction(new AddDefaultStorageItemAction<T>(fileSystem, parentFolder, factory, _recyclingBin));
    }

    public Task<IUserActionResult> Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex)
    {
        return actionManager.ExecuteAction(new MoveStorageItemAction(itemToMove, destinationFolder, fileSystem, _recyclingBin, destinationIndex));
    }

    public Task<IUserActionResult> Delete(ILaminarStorageItem itemToDelete)
    {
        return actionManager.ExecuteAction(new DeleteStorageItemAction(itemToDelete, fileSystem, _recyclingBin)); 
    }

    public Task<IUserActionResult> Rename(ILaminarStorageItem itemToRename, string newName)
    {
        return actionManager.ExecuteAction(new RenameStorageItemAction(newName, itemToRename, fileSystem, _recyclingBin));
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