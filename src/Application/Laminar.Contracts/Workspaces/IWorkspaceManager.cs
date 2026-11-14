using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Workspaces;

public interface IWorkspaceManager
{
    public IReadOnlyObservableList<IWorkspace> Workspaces { get; }
    
    public Task<UserActionResult> AddWorkspace(FileSystemPath workspacePath);
    
    public Task<UserActionResult> RemoveWorkspace(FileSystemPath workspacePath);

    public event PluginChangedEventHandler? PluginChanged;
}