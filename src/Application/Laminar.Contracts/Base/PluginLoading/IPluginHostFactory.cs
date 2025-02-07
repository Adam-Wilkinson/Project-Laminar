using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginHostFactory
{
    public IPluginHost GetPluginHost(IRegisteredPlugin registeredPlugin);
}
