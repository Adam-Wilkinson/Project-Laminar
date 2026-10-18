using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;

namespace Laminar.Contracts.Base;

public interface IWorkspaceManager
{
    public IReadOnlyObservableList<IWorkspace> Workspaces { get; }
    
    public IWorkspace CreateWorkspace(string name);

    public event PluginChangedEventHandler? PluginChanged;
}