using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Extensions;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.UserActions;
using static Laminar.Domain.DataManagement.DataLocations;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class FileBrowser : IFileBrowser, IDisposable
{
    private readonly IFileSystemRootFolder _recyclingBin;
    private readonly IUserActionManager _actionManager;
    private readonly IFileSystem _fileSystem;
    private readonly FileBrowserActionDependencies _actionDependencies;
    private readonly IDisposable _rootFoldersChangedSubscription;
    
    public FileBrowser(
        IUserActionManager actionManager,
        IFileSystemItemFactory factory,
        IFileSystemCommandService commandService,
        IFileSystemItemRepository repository,
        IPersistentDataManager dataManager,
        IFileSystem fileSystem)
    {
        _actionManager = actionManager;
        _fileSystem = fileSystem;
        _recyclingBin = factory.CreateRootFolder(LocalDataFolder.ChildPath("Recycling Bin"));

        var rootFolderPaths = dataManager
            .GetDataStore(DataStoreKey.PersistentData)
            ["FileBrowser"].GetOrCreateCollection<IPersistentDictionary>()
            ["RootFolders"].GetValueOrInitialize<List<FileSystemPath>>([RoamingDataFolder.ChildPath("Default")]);
        
        RootFolders = rootFolderPaths.ToObservableCollection().ObservableMap(factory.CreateRootFolder);
        _rootFoldersChangedSubscription = RootFolders.SubscribeForEach(onRemoved: folder => folder.Dispose());

        _actionDependencies = new()
        {
            RecyclingBin = _recyclingBin,
            FileSystem = _fileSystem,
            RootFolders = rootFolderPaths,
            CommandService = commandService,
            ItemRepository = repository
        };
        
        actionManager.RegisterSimplifier(new StorageActionSimplifier(_actionDependencies));
    }

    public IReadOnlyObservableCollection<IFileSystemRootFolder> RootFolders { get; }

    public Task<IUserActionResult> RemoveRootFolder(FileSystemPath rootFolderPath) 
        => _actionManager.ExecuteAction(new RemoveRootFolderAction(rootFolderPath, false, _actionDependencies));

    public Task<IUserActionResult> AddRootFolder(FileSystemPath newRootFolderPath) 
        => _actionManager.ExecuteAction(new AddRootFolderAction(newRootFolderPath, _actionDependencies));

    public Task<IUserActionResult> Add(string itemName, IFileSystemFolder parent, int indexInParent, FileSystemItemType type)
        => _actionManager.ExecuteAction(new AddStorageItemAction(itemName, parent, indexInParent, type, _actionDependencies));

    public Task<IUserActionResult> Move(IFileSystemItem itemToMove, IFileSystemFolder destinationFolder, int destinationIndex) 
        => _actionManager.ExecuteAction(new MoveStorageItemAction(itemToMove, destinationFolder, destinationIndex, _actionDependencies));

    public Task<IUserActionResult> Delete(IFileSystemItem itemToDelete) 
        => _actionManager.ExecuteAction(new DeleteStorageItemAction(itemToDelete, _actionDependencies));

    public Task<IUserActionResult> Rename(IFileSystemItem itemToRename, string newName) 
        => _actionManager.ExecuteAction(new RenameStorageItemAction(newName, itemToRename, _actionDependencies));

    public bool OpenInSystemFileBrowser(IFileSystemItem item) => _fileSystem.OpenInSystemFileBrowser(item.Path);

    public void Dispose()
    {
        _recyclingBin.Dispose();
        _rootFoldersChangedSubscription.Dispose();
        GC.SuppressFinalize(this);
    }
}