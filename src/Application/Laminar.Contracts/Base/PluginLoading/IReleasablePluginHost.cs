using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IReleasablePluginHost : IPluginHost
{
    public void UnregisterAll();
}