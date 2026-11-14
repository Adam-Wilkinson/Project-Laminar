using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Contracts.Workspaces;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Extensions;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Workspaces.Actions;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Workspaces;

internal sealed class WorkspaceManager(
    IPersistentDataManager dataManager,
    IUserActionManager userActionManager,
    IServiceProvider serviceProvider) 
    : IWorkspaceManager, IDisposable
{
    private const string WorkspaceFileName = "laminar.workspace";
    private static readonly FileSystemPath DefaultWorkspace = DataLocations.RoamingDataFolder.ChildPath("Default").ChildPath(WorkspaceFileName);

    private readonly IPersistentValue<List<FileSystemPath>> _workspaces = dataManager
        .GetDataStore(DataStoreKey.PersistentData)
        ["Workspaces"].GetValueOrInitialize<List<FileSystemPath>>([DefaultWorkspace]);
    
    public IReadOnlyObservableList<IWorkspace> Workspaces 
        => field ??= _workspaces.ToObservableList().ObservableMap(CreateWorkspaceFromPath);

    public Task<UserActionResult> AddWorkspace(FileSystemPath workspacePath)
        => userActionManager.GetScope(IFileBrowser.ActionScopeKey).ExecuteAction(new CreateWorkspaceAction(_workspaces, workspacePath));

    public Task<UserActionResult> RemoveWorkspace(FileSystemPath workspacePath)
        => userActionManager.GetScope(IFileBrowser.ActionScopeKey).ExecuteAction(new RemoveWorkspaceAction(_workspaces, workspacePath));
    
    private void PluginManagerOnPluginChanged(object? sender, PluginChangedEventArgs e)
    {
        PluginChanged?.Invoke(sender, e);
    }

    public event PluginChangedEventHandler? PluginChanged;

    public void Dispose()
    {
        foreach (var workspace in Workspaces)
            workspace.PluginManager.InstalledPlugins.Changed -= PluginManagerOnPluginChanged;
    }

    private IWorkspace CreateWorkspaceFromPath(FileSystemPath workspacePath)
    {
        var newWorkspace = ActivatorUtilities.CreateInstance<Workspace>(serviceProvider, workspacePath);
        newWorkspace.PluginManager.InstalledPlugins.Changed += PluginManagerOnPluginChanged;
        return newWorkspace;
    }
}