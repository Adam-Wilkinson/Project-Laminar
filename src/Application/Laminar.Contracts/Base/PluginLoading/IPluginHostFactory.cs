using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginHostFactory
{
    public IPluginHost GetPluginhost(IRegisteredPlugin registeredPlugin);
}
