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
    private readonly List<VersionedPluginId> _plugins = [];
    
    public string Name => "Offline Cache";

    public async IAsyncEnumerable<VersionedPluginId> Reload()
    {
        await Task.CompletedTask;
        
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

    public IReadOnlyList<VersionedPluginId> Plugins => _plugins;

    public Task<IInstalledPlugin> InstallPlugin(IPluginInfo plugin, SemanticVersion version, IRuntimeHost runtimeHost,
        CancellationToken cancellationToken = default)
    {
        var pluginPath = context.OfflineCacheLocation.ChildPath(plugin.Id).ChildPath(version.ToString());
        return pluginInstaller.InstallFromFolder(pluginPath, plugin, version, runtimeHost);
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