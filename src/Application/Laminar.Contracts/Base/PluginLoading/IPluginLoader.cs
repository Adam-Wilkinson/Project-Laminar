using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginLoader
{
    public void Register(IPlugin plugin);
    
    List<IRegisteredPlugin> RegisteredPlugins { get; }
}