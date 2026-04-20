using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Extensions;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.UserActions;
using static Laminar.Domain.DataManagement.DataLocations;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class LaminarFileBrowser(
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
        .GetOrCreateChild<IPersistentDictionary>("FileBrowser")
        .InitializeDefaultValue<List<FileSystemPath>>(nameof(RootFolders), [RoamingDataFolder.ChildPath("Default")])
        .ToObservableCollection()
        .ObservableMap(factory.CreateRootFolder);
    
    public Task<IUserActionResult> AddDefault<T>(ILaminarStorageFolder parentFolder) where T : class, ILaminarStorageItem 
        => actionManager.ExecuteAction(new AddDefaultStorageItemAction<T>(parentFolder, factory, _recyclingBin));

    public Task<IUserActionResult> Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex) 
        => itemToMove is not LaminarStorageItem internalItem ? Task.FromResult(IUserActionResult.Invalid()) 
            : actionManager.ExecuteAction(new MoveStorageItemAction(internalItem, destinationFolder, _recyclingBin, destinationIndex));

    public Task<IUserActionResult> Delete(ILaminarStorageItem itemToDelete) 
        => itemToDelete is not LaminarStorageItem internalItem ? Task.FromResult(IUserActionResult.Invalid()) 
            : actionManager.ExecuteAction(new DeleteStorageItemAction(internalItem, _recyclingBin));

    public Task<IUserActionResult> Rename(ILaminarStorageItem itemToRename, string newName) 
        => itemToRename is not LaminarStorageItem internalItem ? Task.FromResult(IUserActionResult.Invalid()) 
            : actionManager.ExecuteAction(new RenameStorageItemAction(newName, internalItem, _recyclingBin));

    public bool OpenInSystemFileBrowser(ILaminarStorageItem item) => fileSystem.OpenInSystemFileBrowser(item.Path);

    public void Dispose()
    {
        foreach (ILaminarStorageRootFolder rootFolder in RootFolders)
        {
            rootFolder.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}