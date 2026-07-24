using System.Collections.Specialized;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.UserActions;
using static Laminar.Domain.DataManagement.DataLocations;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class FileBrowser : IFileBrowser, IDisposable
{
    private static readonly FileSystemPath DefaultRootFolder = RoamingDataFolder.ChildPath("Default");
    
    private readonly IUserActionManager _actionManager;
    private readonly IFileSystem _fileSystem;
    private readonly FileBrowserActionDependencies _actionDependencies;
    private readonly IPersistentValue<List<FileSystemPath>> _rootFolderPaths;
    
    public FileBrowser(
        IUserActionManager actionManager,
        IFileSystemGraph graph,
        IFileSystemCommandService commandService,
        IPersistentDataManager dataManager,
        IFileSystem fileSystem,
        IExceptionHandler exceptionHandler)
    {
        _actionManager = actionManager;
        _fileSystem = fileSystem;

        _rootFolderPaths = dataManager
            .GetDataStore(DataStoreKey.PersistentData)
            ["FileBrowser"].GetOrCreateCollection<IPersistentDictionary>()
            ["RootFolders"].GetValueOrInitialize<List<FileSystemPath>>([DefaultRootFolder]);

        foreach (var path in _rootFolderPaths.Value)
        {
            if (!fileSystem.Exists(path) && path != DefaultRootFolder)
            {
                exceptionHandler.OnException(new DirectoryNotFoundException($"The root folder at '{path}' could not be found on disk"));
                continue;
            }
            
            graph.Roots.AddRoot(path);
        }

        RootFolders = graph.Roots;
        RootFolders.CollectionChanged += OnRootsChanged;

        _actionDependencies = new()
        {
            FileSystem = _fileSystem,
            Graph = graph,
            CommandService = commandService,
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
    
    private void OnRootsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _rootFolderPaths.Value = RootFolders.Select(x => x.Path).ToList();
    }
    
    public void Dispose()
    {
        RootFolders.CollectionChanged -= OnRootsChanged;
        GC.SuppressFinalize(this);
    }
}