using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.Graph;
using Laminar.Implementation.Storage.PersistentData;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class FileSystemRootFolder : FileSystemFolder, IMutableFileSystemRootFolder
{
    private const string InfoFileName = ".laminar.data";
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemMonitor _fileSystemMonitor;
    private readonly IDataOnDisk<IPersistentDictionary> _persistentDataOnDisk;
    
    private FileSystemPath _path;
    private IDisposable _currentMonitor;
    private bool _isDisposed;
    
    public FileSystemRootFolder(
        FileSystemPath path, 
        IPersistentDictionary persistentData,
        IFileSystem fileSystem,
        IPersistentDataManager persistentDataManager,
        IFileSystemMonitor monitor,
        IPluginInstaller pluginInstaller,
        IFileSystemGraph graph) 
        : base(persistentData, fileSystem, graph)
    {
        _path = path;
        _fileSystem = fileSystem;
        _fileSystemMonitor = monitor;
        _persistentDataOnDisk = persistentDataManager.GetDataOnDisk(path.ChildPath(InfoFileName), new JsonPersistentDataTranscoder(null!), persistentData);
        _currentMonitor = monitor.StartMonitoring(this, [ _persistentDataOnDisk.Location ]);

        foreach (var requiredPlugin in persistentData["required-plugins"].GetOrCreateCollection<IPersistentList>())
        {
            var pluginInfo = requiredPlugin.GetOrCreateCollection<IPersistentDictionary>();
            var pluginId = pluginInfo["id"].GetValue<string>().Value;
            var version = pluginInfo["version"].GetValue<SemanticVersion>().Value;
            pluginInstaller.EnsurePluginInstalled(pluginId, version);
        }
        
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

        _persistentDataOnDisk.Location = _path.ChildPath(InfoFileName);
        _currentMonitor.Dispose();
        _currentMonitor = _fileSystemMonitor.StartMonitoring(this, [ _persistentDataOnDisk.Location ]);
        OnPropertyChanged(nameof(Path));
    }

    public void OnRemoved(FileSystemGraph.MutationToken _, bool cleanupInfoFiles)
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _currentMonitor.Dispose();
        _persistentDataOnDisk.Dispose();
        
        if (cleanupInfoFiles)
        {
            _fileSystem.Delete(_persistentDataOnDisk.Location);
        }
        
        OnDeleted();
    }
}