using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.Infrastructure;
using Laminar.Implementation.Storage.PersistentData;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class FileSystemRootFolder : FileSystemFolder, IFileSystemRootFolder
{
    private const string InfoFileName = ".laminar.data";
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemMonitor _fileSystemMonitor;
    private readonly IDataOnDisk<IPersistentDictionary> _persistentData;
    
    private FileSystemPath _path;
    private IDisposable _currentMonitor;
    private bool _isDisposed;
    
    public FileSystemRootFolder(
        FileSystemPath path, 
        IPersistentDictionary persistentData,
        IFileSystemItemRepository itemRepository,
        IFileSystem fileSystem,
        IPersistentDataManager persistentDataManager,
        IFileSystemMonitor monitor,
        IFileSystemGraph graph) 
        : base(persistentData, itemRepository, fileSystem, graph)
    {
        _path = path;
        _fileSystem = fileSystem;
        _fileSystemMonitor = monitor;
        _persistentData = persistentDataManager.GetDataOnDisk(path.ChildPath(InfoFileName), new JsonPersistentDataTranscoder(null!), persistentData);
        _currentMonitor = monitor.StartMonitoring(this, [ _persistentData.Location ]);
        Refresh();
    }

    public override FileSystemPath Path => _path;

    public override void SetNameInternal(FileSystemGraph.MutationToken _, string newNameWithExtension)
    {
        if (Path.NameAndExtension == newNameWithExtension) return;
        
        if (Path.Parent is not { } parentPath) 
            throw new InvalidOperationException();

        _path = parentPath.ChildPath(newNameWithExtension);
        PersistentStorage[IFileSystemItemFactory.PersistenceNameKey].GetValue<string>().Value = newNameWithExtension;

        _persistentData.Location = _path.ChildPath(InfoFileName);
        _currentMonitor.Dispose();
        _currentMonitor = _fileSystemMonitor.StartMonitoring(this, [ _persistentData.Location ]);
        OnPropertyChanged(nameof(Path));
    }
    
    public void Dispose(bool cleanupInfoFiles)
    {
        Dispose();
        if (cleanupInfoFiles)
        {
            _fileSystem.Delete(_persistentData.Location);
        }
    }
    
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _currentMonitor.Dispose();
        _persistentData.Dispose();
        OnParentRootFolderDisposed(this, EventArgs.Empty);
        GC.SuppressFinalize(this);
    }
}