using Laminar_PluginFramework.Registration;
using System;
using System.IO;
using System.Reflection;

namespace Laminar_Core.PluginManagement
{
    public class PluginLoader
    {
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
                        plugin.Register(pluginHost);
                    }
                }
            }
        }
    }
}
