using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;

namespace Laminar.Contracts.Base;

public interface IWorkspaceManager
{
    public IReadOnlyObservableList<IWorkspace> Workspaces { get; }
    
    public IWorkspace CreateWorkspace(string name);

    public event EventHandler<PluginsChangedEventArgs>? PluginsChanged;
}

public class PluginsChangedEventArgs : EventArgs
{
    public required IWorkspace ChangedWorkspace { get; init; }
    
    public required string PluginId { get; init; }
    
    public required PluginChangedType ChangeType { get; init; } 
}

public enum PluginChangedType
{
    Added,
    Removed
}