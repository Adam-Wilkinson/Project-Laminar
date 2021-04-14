using OpenFlow_PluginFramework.Registration;
using System;
using System.IO;
using System.Reflection;

namespace OpenFlow_Core.PluginManagement
{
    public class PluginLoader
    {
        private readonly IPluginHost pluginHost = new PluginHost();

        public PluginLoader()
        {
            foreach (string pluginPath in InbuiltPluginFinder.GetInbuiltPlugins())
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
