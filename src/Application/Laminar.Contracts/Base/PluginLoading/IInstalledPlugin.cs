using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IInstalledPlugin
{
    public IPluginInfo PluginInfo { get; }
    
    public SemanticVersion Version { get; }

    public IRuntimeHost Host { get; }
}