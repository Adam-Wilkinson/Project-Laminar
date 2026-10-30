using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Domain.Observables.Collections;

namespace Laminar.Contracts.Workspaces;

public interface IWorkspaceManager
{
    public IReadOnlyObservableList<IWorkspace> Workspaces { get; }
    
    public IWorkspace CreateWorkspace(string name);

    public event PluginChangedEventHandler? PluginChanged;
}