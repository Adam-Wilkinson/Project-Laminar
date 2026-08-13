using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Implementation.Base.PluginLoading.Repositories;

public class OfflineCacheSource(
    IFileSystem fileSystem,
    ISharedPluginContext context,
    IPluginInstaller pluginInstaller) : IPluginSource
{
    private readonly HashSet<VersionedPluginId> _plugins = [];
    
    public string Name => "Offline Cache";

    public async IAsyncEnumerable<VersionedPluginId> Reload()
    {
        await Task.CompletedTask;
        
        if (!fileSystem.Exists(context.OfflineCacheLocation))
        {
            fileSystem.CreateDirectory(context.OfflineCacheLocation);
        }
        else if (!fileSystem.IsDirectory(context.OfflineCacheLocation))
        {
            throw new DirectoryNotFoundException(
                $"Could not create offline cache '{context.OfflineCacheLocation}' due to an existing file");
        }
        
        foreach (var pluginFolder in fileSystem.EnumerateChildren(context.OfflineCacheLocation))
        {
            if (!fileSystem.IsDirectory(pluginFolder))
                continue;
            
            var pluginName = pluginFolder.NameAndExtension;
            foreach (var versionFolder in fileSystem.EnumerateChildren(pluginFolder))
            {
                if (!fileSystem.IsDirectory(pluginFolder) ||
                    !SemanticVersion.TryParse(versionFolder.NameAndExtension, out var version)) continue;
                
                _plugins.Add(new VersionedPluginId(versionFolder.NameAndExtension, version));
                yield return new VersionedPluginId(pluginName, version);
            }
        }
    }

    public IReadOnlyCollection<VersionedPluginId> Plugins => _plugins;

    public bool HasPlugin(VersionedPluginId plugin) => _plugins.Contains(plugin);

    public Task<IInstalledPlugin> InstallPlugin(VersionedPluginId pluginId, IRuntimeHost runtimeHost,
        CancellationToken cancellationToken = default)
    {
        var pluginPath = context.OfflineCacheLocation.ChildPath(pluginId.Name).ChildPath(pluginId.Version.ToString());
        return pluginInstaller.InstallFromFolder(pluginPath, pluginId, runtimeHost);
    }

    public async Task<ManifestData> GetManifest(VersionedPluginId plugin, CancellationToken cancellationToken = default)
    {
        var manifestPath = context.OfflineCacheLocation
            .ChildPath(plugin.Name)
            .ChildPath(plugin.Version.ToString())
            .ChildPath("manifest.json");
        
        await using var manifestStream = File.OpenRead(manifestPath);
        return ManifestData.Parse(manifestStream);
    }
}