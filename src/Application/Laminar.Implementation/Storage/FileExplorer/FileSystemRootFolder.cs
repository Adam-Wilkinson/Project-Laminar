using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Contracts.Workspaces;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer.Graph;
using Laminar.Implementation.Storage.FileExplorer.Notifications;
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
        IWorkspaceManager workspaceManager,
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
        Workspace = workspaceManager.CreateWorkspace(UserFriendlyName);
        _ = RunBackgroundStartup();
        
        Refresh();
    }

    public override FileSystemPath Path => _path;

    public IWorkspace Workspace { get; }

    public override void SetNameInternal(FileSystemGraph.MutationToken _, string newNameWithExtension)
    {
        if (Path.NameAndExtension == newNameWithExtension) return;
        
        if (Path.Parent is not { } parentPath) 
            throw new InvalidOperationException();

        _path = parentPath.ChildPath(newNameWithExtension);
        Name.Value = newNameWithExtension;

        _persistentDataOnDisk.Location = _path.ChildPath(InfoFileName);
        _currentMonitor.Dispose();
        _currentMonitor = _fileSystemMonitor.StartMonitoring(this, [ _persistentDataOnDisk.Location ]);
        Workspace.Name = UserFriendlyName;
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
        if (!pluginInfo["id"].GetValue<string>().TryGetResult(out var pluginId, out var idError))
        {
            await _exceptionHandler.OnExceptionAsync(new Exception("Unable to get plugin id", idError));
            return;
        }
        
        if (!pluginInfo["version"].GetValue<SemanticVersion>().TryGetResult(out var version, out var versionError))
        {
            await _exceptionHandler.OnExceptionAsync(new Exception("Unable to get plugin version", versionError));
            return;
        }
        
        var versionedPluginId = new VersionedPluginId(pluginId.Value, version.Value); 
        bool loadedPlugin;
        using (var _ = NotificationManager.AddNotification(new LoadingRequiredPluginNotification(versionedPluginId)))
        {
            loadedPlugin = await Workspace.PluginManager.EnsurePluginInstalled(versionedPluginId);
        }
        
        if (!loadedPlugin)
        {
            NotificationManager.AddNotification(new PluginNotFoundNotification(versionedPluginId));
        }
    }
}