using Laminar.Contracts.Base.PluginLoading;

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
}