using System.Runtime.Loader;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginStartupService(
    IFileSystem fileSystem,
    IPluginLoader pluginLoader, 
    IWritablePluginRegistry pluginRegistry,
    ILogger<IPluginStartupService> logger) : IPluginStartupService
{
    private static readonly FileSystemPath PluginPath = new FileSystemPath(AppContext.BaseDirectory).ChildPath("plugins");
    
    public void Initialize(FrontendDependency frontend, AssemblyLoadContext? defaultLoadContext)
    {
        if (!fileSystem.Exists(PluginPath))
        {
            logger.LogError("No plugins folder found under '{AbsolutePluginPath}'. Creating it and then loading no plugins, but this is likely a fatal error", PluginPath);
            fileSystem.CreateDirectory(PluginPath);
            return;
        }
        
        defaultLoadContext ??= AssemblyLoadContext.Default;
        
        foreach (var pluginDirectory in fileSystem.EnumerateChildren(PluginPath).Where(fileSystem.IsDirectory))
        {
            foreach (var registeredPlugin in pluginLoader.LoadFrom(pluginDirectory, frontend, defaultLoadContext))
            {
                pluginRegistry.RegisterPlugin(registeredPlugin);
            }
        }
    }
}