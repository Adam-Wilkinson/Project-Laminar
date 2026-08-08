using System.Reflection;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInstaller
{
    public IEnumerable<IRegisteredPlugin> LoadFrom(FileSystemPath pluginPath);

    public Task<Assembly?> TryGetPluginAssembly(IPluginInfo pluginInfo, SemanticVersion? versionOverride);
    
    /// <summary>
    /// <para>Ensures that the plugin is installed within the runtime.</para>
    ///
    /// <para>For asynchronous plugin version loading, this returns true if or when the plugin is loaded
    /// and false when all plugin sources have been loaded without finding the request</para> 
    /// </summary>
    /// <param name="pluginId">The unique ID of the plugin</param>
    /// <param name="version">The required version of the plugin</param>
    /// <returns>True if the plugin is installed, false if not</returns>
    public Task<bool> EnsurePluginInstalled(string pluginId, SemanticVersion version);
}