using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Contracts.UserInterface;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Registration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Laminar.Core.PluginManagement
{
    public class PluginLoader
    {
        private readonly string[] AutoLoadPlugins = { "Base plugin functionality", "Keyboard and Mouse interface", "Windows Base" };

        public PluginLoader(string path, IUserInterfaceStore userInterfaceStore, ITypeInfoStore typeInfoStore, ILoadedNodeManager loadedNodeManager, IConnectorViewFactory connectorViewFactory)
        {
            List<IRegisteredPlugin> registeredPlugins = new();
            foreach (string pluginPath in InbuiltPluginFinder.GetInbuiltPlugins(path))
            {
                PluginLoadContext pluginContext = new(pluginPath);
                Assembly pluginAssembly = pluginContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginPath)));
                foreach (Type type in pluginAssembly.GetExportedTypes())
                {
                    if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface)
                    {
                        IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                        RegisteredPlugin registeredPlugin = new(plugin, loadedNodeManager, typeInfoStore, userInterfaceStore, connectorViewFactory);
                        if (AutoLoadPlugins.Contains(registeredPlugin.PluginName))
                        {
                            registeredPlugin.Load();
                        }
                        if (registeredPlugin.PluginName == "Base plugin functionality")
                        {
                            registeredPlugin.RegisterNode(new InputNode());
                        }
                        registeredPlugins.Add(registeredPlugin);

                    }
                }
            }
            RegisteredPlugins = registeredPlugins.ToArray();
        }

        public IRegisteredPlugin[] RegisteredPlugins { get; }

        public class RegisteredPlugin : IRegisteredPlugin
        {
            readonly IPluginHost _host;
            readonly IPlugin _plugin;
            readonly Dictionary<string, Type> _registeredNodesByName = new();
            readonly HashSet<Type> _registeredNodes = new();

            public RegisteredPlugin(IPlugin plugin, ILoadedNodeManager loadedNodeManager, ITypeInfoStore typeInfoStore, IUserInterfaceStore userInterfaceStore, IConnectorViewFactory connectorViewFactory)
            {
                _host = new PluginHost(this, typeInfoStore, loadedNodeManager, userInterfaceStore, connectorViewFactory);
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
}
