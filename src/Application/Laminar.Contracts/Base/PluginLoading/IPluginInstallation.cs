using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInstallation : IPluginHost
{
    public void Uninstall();
    
    public VersionedPluginId PluginId { get; }
}