using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base;

public interface IRuntimeHostManager
{
    public IReadOnlyCollection<IRuntimeHost> AllHosts { get; }
    
    public IRuntimeHost CreateRuntimeHost(string name);

    public event EventHandler<PluginsChangedEventArgs>? PluginsChanged;
}

public class PluginsChangedEventArgs : EventArgs
{
    public required IRuntimeHost ChangedRuntime { get; init; }
    
    public required string PluginId { get; init; }
    
    public required PluginChangedType ChangeType { get; init; } 
}

public enum PluginChangedType
{
    Added,
    Removed
}