using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Reflection;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginManager(
    IRuntimeHost host,
    IPluginHostFactory hostFactory, 
    IPluginInstallContext installContext,
    IPluginRepositoryStore repositoryStore,
    IFileSystem fileSystem) 
    : IPluginManager
{
    private static readonly FileSystemPath OfflinePluginCache = DataLocations.LocalDataFolder.ChildPath("Plugins");
    private readonly ObservableCollection<IInstalledPlugin> _plugins = [];
    private readonly Dictionary<VersionedPluginId, InstalledPlugin> _installedPlugins = [];
    
    public IReadOnlyObservableCollection<IInstalledPlugin> Plugins => field ??= _plugins.ToInterfaceImpl();
    
    public async Task<IInstalledPlugin?> EnsurePluginInstalled(VersionedPluginId pluginId)
    {
        if (_installedPlugins.TryGetValue(pluginId, out var plugin))
        {
            return plugin;
        }

        var pluginPath = OfflinePluginCache.ChildPath(pluginId.Name).ChildPath(pluginId.Version.ToString());
        if (!fileSystem.Exists(pluginPath))
        {
            if (await repositoryStore.GetPluginInfoOrNull(pluginId) is not { } pluginInfo)
            {
                return null;
            }
            
            fileSystem.CreateDirectory(pluginPath);
            await using var stream = await pluginInfo.OpenVersionStream(pluginId.Version);
            await ZipFile.ExtractToDirectoryAsync(stream, pluginPath);
        }
        
        var manifestPath = pluginPath.ChildPath("manifest.json");
        if (!fileSystem.Exists(manifestPath)) throw new InvalidOperationException("Could not find plugin manifest");
        await using var manifestFile = File.OpenRead(manifestPath);
        var manifest = ManifestData.Parse(manifestFile);
        var entrypointPath = pluginPath.ChildPath((string)manifest.Entrypoint);
        var pluginLoadContext = new PluginLoadContext(entrypointPath, installContext.DefaultLoadContext);
        var pluginAssembly = pluginLoadContext.LoadFromAssemblyPath(entrypointPath);

        var newPlugin = new InstalledPlugin(hostFactory, host.NodeManager);
        foreach (var type in pluginAssembly.GetTypes())
        {
            if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface &&
                type.GetConstructor(BindingFlags.Public, []) is not null
                && Activator.CreateInstance(type) is IPlugin pluginFront)
            {
                newPlugin.AddPluginImplementation(pluginFront);
            }
        }

        _plugins.Add(newPlugin);
        _installedPlugins.Add(pluginId, newPlugin);
        return newPlugin;
    }
}