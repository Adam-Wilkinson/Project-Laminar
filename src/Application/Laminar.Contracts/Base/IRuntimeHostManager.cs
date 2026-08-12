using Laminar.Contracts.Base.PluginLoading;

namespace Laminar.Contracts.Base;

public interface IRuntimeHostManager
{
    public IReadOnlyCollection<IRuntimeHost> AllHosts { get; }
    
    public IRuntimeHost CreateRuntimeHost(string name);
}