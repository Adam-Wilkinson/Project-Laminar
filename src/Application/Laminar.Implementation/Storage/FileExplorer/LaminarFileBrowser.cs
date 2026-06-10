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
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.UserActions;
using static Laminar.Domain.DataManagement.DataLocations;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class LaminarFileBrowser : ILaminarFileBrowser, IDisposable
{
    private readonly ILaminarStorageRootFolder _recyclingBin;
    private readonly IUserActionManager _actionManager;
    private readonly IFileSystem _fileSystem;
    private readonly FileExplorerActionDependencies _actionDependencies;
    private readonly IDisposable _rootFoldersChangedSubscription;
    
    public LaminarFileBrowser(
        IUserActionManager actionManager,
        ILaminarStorageItemFactory factory,
        IPersistentDataManager dataManager,
        IFileSystem fileSystem)
    {
        _actionManager = actionManager;
        _fileSystem = fileSystem;
        _recyclingBin = factory.CreateRootFolder(LocalDataFolder.ChildPath("Recycling Bin"));

        var rootFolderPaths = dataManager
            .GetDataStore(DataStoreKey.PersistentData)
            ["FileBrowser"].GetOrCreateCollection<IPersistentDictionary>()
            ["RootFolders"].GetValueOrDefault<List<FileSystemPath>>([RoamingDataFolder.ChildPath("Default")]);
        
        RootFolders = rootFolderPaths.ToObservableCollection().ObservableMap(factory.CreateRootFolder);
        _rootFoldersChangedSubscription = RootFolders.SubscribeForEach(onRemoved: folder => folder.Dispose());

        _actionDependencies = new()
        {
            RecyclingBin = _recyclingBin,
            FileSystem = _fileSystem,
            RootFolders = rootFolderPaths,
            StorageItemFactory = factory
        };
        
        actionManager.RegisterSimplifier(new StorageActionSimplifier(_actionDependencies));
    }

    public IReadOnlyObservableCollection<ILaminarStorageRootFolder> RootFolders { get; }

    public Task<IUserActionResult> RemoveRootFolder(FileSystemPath rootFolderPath) 
        => _actionManager.ExecuteAction(new RemoveRootFolderAction(rootFolderPath, false, _actionDependencies));

    public Task<IUserActionResult> AddRootFolder(FileSystemPath newRootFolderPath) 
        => _actionManager.ExecuteAction(new AddRootFolderAction(newRootFolderPath, _actionDependencies));

    public Task<IUserActionResult> Add(string itemName, ILaminarStorageFolder parent, int indexInParent, StorageItemType type)
        => parent is not LaminarStorageFolder internalParent ? Task.FromResult(IUserActionResult.Invalid()) :
            _actionManager.ExecuteAction(new AddStorageItemAction(itemName, internalParent, indexInParent, type, _actionDependencies));

    public Task<IUserActionResult> Move(ILaminarStorageItem itemToMove, ILaminarStorageFolder destinationFolder, int destinationIndex) 
        => itemToMove is not LaminarStorageItem internalItem ? Task.FromResult(IUserActionResult.Invalid()) 
            : _actionManager.ExecuteAction(new MoveStorageItemAction(internalItem, destinationFolder, destinationIndex, _actionDependencies));

    public Task<IUserActionResult> Delete(ILaminarStorageItem itemToDelete) 
        => itemToDelete is not LaminarStorageItem internalItem ? Task.FromResult(IUserActionResult.Invalid()) 
            : _actionManager.ExecuteAction(new DeleteStorageItemAction(internalItem, _actionDependencies));

    public Task<IUserActionResult> Rename(ILaminarStorageItem itemToRename, string newName) 
        => itemToRename is not LaminarStorageItem internalItem ? Task.FromResult(IUserActionResult.Invalid()) 
            : _actionManager.ExecuteAction(new RenameStorageItemAction(newName, internalItem, _actionDependencies));

    public bool OpenInSystemFileBrowser(ILaminarStorageItem item) => _fileSystem.OpenInSystemFileBrowser(item.Path);

    public void Dispose()
    {
        _recyclingBin.Dispose();
        _rootFoldersChangedSubscription.Dispose();
        GC.SuppressFinalize(this);
    }
}