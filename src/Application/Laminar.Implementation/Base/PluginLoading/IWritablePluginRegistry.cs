using Laminar.Contracts.Base.PluginLoading;

namespace Laminar.Implementation.Base.PluginLoading;

public interface IWritablePluginRegistry : IPluginRegistry
{
    public void RegisterPlugin(IRegisteredPlugin plugin);
    
    public void UnregisterPlugin(IRegisteredPlugin plugin);
}