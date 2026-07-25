using System.Reflection;
using System.Runtime.Loader;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Base.PluginLoading;

internal sealed class PluginLoader(
    IPluginHostFactory pluginHostFactory, 
    IFileSystem fileSystem,
    ILogger<IPluginHost> logger) 
    : IPluginLoader
{
    public IEnumerable<IRegisteredPlugin> LoadFrom(FileSystemPath pluginPath, FrontendDependency frontendDependency, AssemblyLoadContext defaultLoadContext)
    {
        var pluginName = fileSystem.GetNameWithoutExtension(pluginPath);
        var pluginDllPath = pluginPath.ChildPath(pluginName + ".dll");
        PluginLoadContext pluginContext = new(pluginDllPath, defaultLoadContext);
        var pluginAssembly = pluginContext.LoadFromAssemblyPath(pluginDllPath);
        foreach (var module in pluginAssembly.Modules)
        {
            foreach (var plugin in GetPluginsFrom(module, frontendDependency))
            {
                RegisteredPlugin? registeredPlugin = null;
                try
                {
                    registeredPlugin = new RegisteredPlugin(plugin, pluginHostFactory, pluginAssembly);
                    registeredPlugin.Load();
                }
                catch (Exception e)
                { 
                    logger.LogError(e, "Error loading module '{moduleName}'", module.Name);
                }

                if (registeredPlugin is not null)
                {
                    yield return registeredPlugin;
                }
            }
        }
    }

    private static IEnumerable<IPlugin> GetPluginsFrom(Module module, FrontendDependency frontendDependency)
    {
        if (module.GetCustomAttribute<HasFrontendDependencyAttribute>() is { } attr && attr.FrontendDependency != frontendDependency)
        {
            yield break;
        }

        foreach (var type in module.GetTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type) || type.IsInterface ||
                Activator.CreateInstance(type) is not IPlugin plugin) continue;
            
            yield return plugin;
        }
    }
}
