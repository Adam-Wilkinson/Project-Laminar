using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginLoader(FrontendDependency frontend, IPluginHostFactory pluginHostFactory, ILogger<IPluginHost> logger) : IPluginLoader
{
    private static readonly string[] AutoLoadPlugins = ["Basic Functionality UI", "Base plugin functionality"];//, "Keyboard and Mouse interface", "Windows Base" };
    
    public void Register(IPlugin plugin)
    {
        RegisteredPlugin registeredPlugin = new(plugin, pluginHostFactory);
        if (AutoLoadPlugins.Contains(registeredPlugin.PluginName))
        {
            registeredPlugin.Load();
        }
        
        RegisteredPlugins.Add(registeredPlugin);
    }
    
    public List<IRegisteredPlugin> RegisteredPlugins { get; } = [];
    
    public void LoadInbuiltFromPath(string path)
    {
        foreach (var pluginPath in InbuiltPluginFinder.GetInbuiltPlugins(logger, path))
        {
            PluginLoadContext pluginContext = new(pluginPath);
            var pluginAssembly = pluginContext.LoadFromAssemblyPath(pluginPath);
            foreach (var module in pluginAssembly.Modules)
            {
                try
                {
                    LoadModule(module);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error loading module {module.Name}: {e}");
                }
            }
        }
    }
    
    private void LoadModule(Module module)
    {
        if (module.GetCustomAttribute<HasFrontendDependencyAttribute>() is { } attr && attr.FrontendDependency != frontend)
        {
            return;
        }

        foreach (var type in module.GetTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type) || type.IsInterface ||
                Activator.CreateInstance(type) is not IPlugin plugin) continue;
            
            Register(plugin);
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
