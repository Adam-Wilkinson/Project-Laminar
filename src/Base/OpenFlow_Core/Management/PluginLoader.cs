namespace OpenFlow_Core.Management
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using OpenFlow_PluginFramework.Registration;

    public static class PluginLoader
    {
        public static bool TryLoadFromFolder(string path, out IPlugin plugin)
        {
            foreach (string pluginFile in Directory.EnumerateFiles(path, "*.dll"))
            {
                Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);
                foreach (Type type in pluginAssembly.GetExportedTypes())
                {
                    if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface)
                    {
                        plugin = (IPlugin)Activator.CreateInstance(type);
                        return true;
                    }
                }
            }

            plugin = default;
            return false;
        }
    }
}
