using System.Reflection;
using System.Runtime.Loader;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Base.PluginLoading;

internal sealed class PluginLoader(
    FrontendDependency frontend,
    AssemblyLoadContext defaultLoadContext,
    IPluginHostFactory pluginHostFactory, 
    ILogger<IPluginHost> logger) 
    : IPluginLoader
{
    private static readonly string PluginPath = Path.Combine(AppContext.BaseDirectory, "plugins");
    private readonly Dictionary<string, IRegisteredPlugin> _registeredPlugins = [];
    
    public void EnsurePluginsLoaded()
    {
        if (!Directory.Exists(PluginPath))
        {
            logger.LogError("No plugins folder found under '{AbsolutePluginPath}'. Creating it and then loading no plugins, but this is likely a fatal error", Path.GetFullPath(PluginPath));
            Directory.CreateDirectory(PluginPath);
            return;
        }
        
        foreach (var pluginDirectory in Directory.EnumerateDirectories(PluginPath))
        {
            var pluginName = Path.GetFileName(pluginDirectory);
            var pluginPath = Path.GetFullPath(Path.Combine(pluginDirectory, pluginName + ".dll"));
            PluginLoadContext pluginContext = new(pluginPath, defaultLoadContext);
            var pluginAssembly = pluginContext.LoadFromAssemblyPath(pluginPath);
            foreach (var module in pluginAssembly.Modules)
            {
                try
                {
                    EnsureModuleLoaded(module);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error loading module '{moduleName}'", module.Name);
                }
            }
        }
    }

    public IRegisteredPlugin GetPluginFor(INode node) => _registeredPlugins.Values.First(plugin => plugin.ContainsNode(node));

    private void EnsureModuleLoaded(Module module)
    {
        if (module.GetCustomAttribute<HasFrontendDependencyAttribute>() is { } attr && attr.FrontendDependency != frontend)
        {
            return;
        }

        foreach (var type in module.GetTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type) || type.IsInterface ||
                Activator.CreateInstance(type) is not IPlugin plugin) continue;
            
            Load(plugin);
        }
    }
    
    private void Load(IPlugin plugin)
    {
        if (_registeredPlugins.ContainsKey(plugin.PluginName))
        {
            return;
        }
        
        RegisteredPlugin registeredPlugin = new(plugin, pluginHostFactory);
        registeredPlugin.Load();
        _registeredPlugins.Add(registeredPlugin.PluginName, registeredPlugin);
    }
}
