using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginLoader
{
    private static readonly string[] AutoLoadPlugins = ["Basic Functionality UI", "Base plugin functionality"];//, "Keyboard and Mouse interface", "Windows Base" };
    private readonly FrontendDependency _frontend;
    private readonly IPluginHostFactory _pluginHostFactory;

    public PluginLoader(string path, FrontendDependency frontend, IPluginHostFactory pluginHostFactory)
    {
        _frontend = frontend;
        _pluginHostFactory = pluginHostFactory;
        List<IRegisteredPlugin> registeredPlugins = [];
        foreach (var pluginPath in InbuiltPluginFinder.GetInbuiltPlugins(path))
        {
            PluginLoadContext pluginContext = new(pluginPath);
            var pluginAssembly = pluginContext.LoadFromAssemblyPath(pluginPath);
            foreach (var module in pluginAssembly.Modules)
            {
                try
                {
                    if (LoadModule(module) is { } plugin)
                    {
                        registeredPlugins.Add(plugin);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error loading module {module.Name}: {e}");
                }
            }
        }

        RegisteredPlugins = registeredPlugins.ToArray();
    }

    private RegisteredPlugin? LoadModule(Module module)
    {
        if (module.GetCustomAttribute<HasFrontendDependencyAttribute>() is { } attr && attr.FrontendDependency != _frontend)
        {
            return null;
        }

        foreach (var type in module.GetTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type) || type.IsInterface ||
                Activator.CreateInstance(type) is not IPlugin plugin) continue;
            
            RegisteredPlugin registeredPlugin = new(plugin, _pluginHostFactory);
            if (AutoLoadPlugins.Contains(registeredPlugin.PluginName))
            {
                registeredPlugin.Load();
            }
            return registeredPlugin;
        }

        return null;
    }

    public IRegisteredPlugin[] RegisteredPlugins { get; }

    private class RegisteredPlugin : IRegisteredPlugin
    {
        private readonly IPluginHost _host;
        private readonly IPlugin _plugin;
        private readonly Dictionary<string, Type> _registeredNodesByName = new();
        private readonly HashSet<Type> _registeredNodes = new();

        public RegisteredPlugin(IPlugin plugin, IPluginHostFactory pluginHostFactory)
        {
            _host = pluginHostFactory.GetPluginhost(this);
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
