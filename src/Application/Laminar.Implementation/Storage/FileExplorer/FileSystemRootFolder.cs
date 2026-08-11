using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notifications;
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
    private readonly IExceptionHandler _exceptionHandler;
    
    private FileSystemPath _path;
    private IDisposable _currentMonitor;
    private bool _isDisposed;
    
    public FileSystemRootFolder(
        FileSystemPath path, 
        IPersistentDictionary persistentData,
        IFileSystem fileSystem,
        IPersistentDataManager persistentDataManager,
        IFileSystemMonitor monitor,
        IRuntimeHost runtimeHost,
        IExceptionHandler exceptionHandler,
        IFileSystemGraph graph) 
        : base(persistentData, fileSystem, graph)
    {
        _path = path;
        _fileSystem = fileSystem;
        _fileSystemMonitor = monitor;
        _persistentDataOnDisk = persistentDataManager.GetDataOnDisk(path.ChildPath(InfoFileName), new JsonPersistentDataTranscoder(null!), persistentData);
        _currentMonitor = monitor.StartMonitoring(this, [ _persistentDataOnDisk.Location ]);
        _exceptionHandler = exceptionHandler;
        RuntimeHost = runtimeHost;
        _ = RunBackgroundStartup();
        
        Refresh();
    }

    public override FileSystemPath Path => _path;

    public IRuntimeHost RuntimeHost { get; }

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

    private async Task RunBackgroundStartup()
    {
        foreach (var pluginData in PersistentStorage["required-plugins"].GetOrCreateCollection<IPersistentList>())
        {
            try
            {
                await InstallPluginFromData(pluginData);
            }
            catch (Exception ex)
            {
                await _exceptionHandler.OnExceptionAsync(ex);
            }
        }
    }

    private async Task InstallPluginFromData(IPersistentDataPoint requiredPlugin)
    {
        var pluginInfo = requiredPlugin.GetOrCreateCollection<IPersistentDictionary>();
        var pluginId = pluginInfo["id"].GetValue<string>().Value;
        var version = pluginInfo["version"].GetValue<SemanticVersion>().Value;
        var versionedPluginId = new VersionedPluginId(pluginId, version); 
        IInstalledPlugin? loadedPlugin;
        using (var _ = NotificationManager.AddNotification(
                   new FileSystemNotifications.LoadingRequiredPlugin(versionedPluginId)))
        {
            loadedPlugin = await RuntimeHost.PluginManager.EnsurePluginInstalled(versionedPluginId);
        }
            
        if (loadedPlugin is null)
        {
            NotificationManager.AddNotification(new FileSystemNotifications.PluginNotFoundError(versionedPluginId));
        }
    }
}