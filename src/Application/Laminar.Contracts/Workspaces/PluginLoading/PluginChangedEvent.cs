namespace Laminar.Contracts.Workspaces.PluginLoading;

public delegate void PluginChangedEventHandler(object? sender, PluginChangedEventArgs e);

public class PluginChangedEventArgs : EventArgs
{
    public required IWorkspace ChangedWorkspace { get; init; }
    
    public required string PluginId { get; init; }
    
    public required PluginChangedType ChangeType { get; init; } 
}

public enum PluginChangedType
{
    Added,
    Removed,
    VersionChanged,
}