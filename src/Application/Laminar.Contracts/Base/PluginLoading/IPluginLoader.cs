using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginLoader
{
    public void Register(IPlugin plugin);
    
    List<IRegisteredPlugin> RegisteredPlugins { get; }
}

public static class PluginLoaderExtensions
{
    extension(IPluginLoader pluginLoader)
    {
        public void Register(IEnumerable<IPlugin> plugins)
        {
            foreach (var item in plugins)
            {
                pluginLoader.Register(item);
            }
        }
    }
}