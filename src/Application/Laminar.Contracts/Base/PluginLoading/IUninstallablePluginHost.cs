using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IUninstallablePluginHost : IPluginHost
{
    public void UnregisterAll();
}