using System.Reflection;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginLoader(FrontendDependency frontend, IPluginHostFactory pluginHostFactory, ILogger<IPluginHost> logger) : IPluginLoader
{
    private const string RelativePluginPath = "plugins";
    private readonly Dictionary<string, IRegisteredPlugin> _registeredPlugins = [];
    
    public void Load(IPlugin plugin)
    {
        if (_registeredPlugins.ContainsKey(plugin.PluginName))
        {
            return;
        }
        
        RegisteredPlugin registeredPlugin = new(plugin, pluginHostFactory);
        registeredPlugin.Load();
        _registeredPlugins.Add(registeredPlugin.PluginName, registeredPlugin);
    }
    
    public void EnsurePluginsLoaded()
    {
        if (!Directory.Exists(RelativePluginPath))
        {
            logger.LogError("No plugins folder found under '{AbsolutePluginPath}'. Creating it and then loading no plugins, but this is likely a fatal error", Path.GetFullPath(RelativePluginPath));
            Directory.CreateDirectory(RelativePluginPath);
            return;
        }
        
        foreach (var pluginDirectory in Directory.EnumerateDirectories(RelativePluginPath))
        {
            var pluginName = Path.GetFileName(pluginDirectory);
            var pluginPath = Path.GetFullPath(Path.Combine(pluginDirectory, pluginName + ".dll"));
            PluginLoadContext pluginContext = new(pluginPath);
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

    private class RegisteredPlugin : IRegisteredPlugin
    {
        private readonly IPluginHost _host;
        private readonly IPlugin _plugin;
        private readonly Dictionary<string, Type> _registeredNodesByName = [];
        private readonly HashSet<Type> _registeredNodes = [];

        public RegisteredPlugin(IPlugin plugin, IPluginHostFactory pluginHostFactory)
        {
            _host = pluginHostFactory.GetPluginHost(this);
            _plugin = plugin;
            PluginName = plugin.PluginName;
            PluginDescription = plugin.PluginDescription;
        }

        public string PluginName { get; }

        public string PluginDescription { get; }

        public void RegisterNode(INode node)
        {
            _registeredNodes.Add(node.GetType());
            _registeredNodesByName.Add(node.NodeName, node.GetType());
        }

        public bool ContainsNode(INode node)
        {
            return _registeredNodes.Contains(node.GetType());
        }

        public IReadOnlyDictionary<string, Type> RegisteredNodes => _registeredNodesByName;

        public void Load()
        {
            _plugin.Register(_host);
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }
    }
}
