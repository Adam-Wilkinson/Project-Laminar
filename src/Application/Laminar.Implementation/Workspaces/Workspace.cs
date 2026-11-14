using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Contracts.Workspaces;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting;
using Laminar.Implementation.Scripting.NodeWrapping;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.Implementation.Workspaces.PluginLoading;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Workspaces;

internal sealed class Workspace : IWorkspace, INotifyPropertyChanged, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileSystemGraph _fileSystemGraph;
    private readonly IDataOnDisk<IPersistentDictionary> _persistentData;
    
    public Workspace(
        FileSystemPath workspaceFilePath,
        IFileSystemGraph graph,
        IFileSystem fileSystem,
        IPersistentDataManager persistentDataManager,
        IServiceProvider serviceProvider,
        IUserActionManager userActionManager)
    {
        _serviceProvider = serviceProvider;
        _fileSystemGraph = graph;
        ActionScope = userActionManager.GetScope(Random.Shared.Next().ToString());
        if (workspaceFilePath.Parent is not { } rootFolderPath) 
            throw new InvalidOperationException("Workspace file does not have a parent");

        if (!fileSystem.Exists(rootFolderPath))
        {
            fileSystem.CreateDirectory(rootFolderPath);
        }
        
        _persistentData = persistentDataManager.GetDataOnDisk<IPersistentDictionary>(workspaceFilePath,
                new JsonPersistentDataTranscoder(null!));
        
        RootFolder = graph.Roots.CreateRootFolder(rootFolderPath,
            _persistentData.Data["FileSystemState"].GetOrCreateCollection<IPersistentDictionary>());
    }

    public required string Name { get; set => SetField(ref field, value); }
    
    public IPluginManager PluginManager => field ??= ActivatorUtilities.CreateInstance<PluginManager>(_serviceProvider, this); 
    
    public IScriptingFactory ScriptingFactory => field ??= ActivatorUtilities.CreateInstance<ScriptingFactory>(_serviceProvider, this); 
    
    public ILoadedNodeManager NodeManager => field ??= ActivatorUtilities.CreateInstance<LoadedNodeManager>(_serviceProvider, this);

    public IUserActionScope ActionScope { get; }

    public IFileSystemRootFolder RootFolder { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        _persistentData.Dispose();
        _fileSystemGraph.Roots.RemoveRootAt(RootFolder.Path, false);
    }
}