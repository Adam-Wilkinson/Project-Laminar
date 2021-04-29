using Laminar_PluginFramework.Registration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Laminar_Core.PluginManagement
{
    public class PluginLoader : IDisposable
    {
        private readonly List<IPlugin> _plugins = new();

        public PluginLoader(IPluginHost pluginHost, string path)
        {
            foreach (string pluginPath in InbuiltPluginFinder.GetInbuiltPlugins(path))
            {
                PluginLoadContext pluginContext = new(pluginPath);
                Assembly pluginAssembly = pluginContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginPath)));
                foreach (Type type in pluginAssembly.GetExportedTypes())
                {
                    if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface)
                    {
                        IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                        _plugins.Add(plugin);
                        plugin.Register(pluginHost);
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (IPlugin plugin in _plugins)
            {
                plugin.Dispose();
            }
        }
    }
}
