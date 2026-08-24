using System.IO.Compression;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Implementation.Base.PluginLoading.Repositories;

public class LocalPluginSource(
    string id, 
    FileSystemPath path, 
    IFileSystem fileSystem,
    IExceptionHandler handler,
    IPluginInstaller installer) : IPluginSource
{
    private readonly Dictionary<VersionedPluginId, RepositoryPluginDetails> _pluginDetails = new();
    
    public string Name { get; } = id;

    public IReadOnlyCollection<VersionedPluginId> Plugins => _pluginDetails.Keys;
    
    public async IAsyncEnumerable<VersionedPluginId> Reload()
    {
        foreach (var pluginFile in fileSystem.EnumerateChildren(path, "*.plpkg"))
        {
            await using var fileStream = File.OpenRead(pluginFile);
            await using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read);
            if (zipArchive.GetEntry("manifest.json") is not { } manifestEntry)
            {
                await handler.OnExceptionAsync(new InvalidPluginFormatException(pluginFile.NameAndExtension));
                continue;
            }

            var manifestStream = await manifestEntry.OpenAsync();
            var manifest = ManifestData.Parse(manifestStream);
            var pluginId = new VersionedPluginId((string)manifest.Id, new SemanticVersion((string)manifest.Version));
            _pluginDetails.Add(pluginId, new RepositoryPluginDetails(manifest, pluginFile));
            yield return pluginId;
        }
    }

    public bool HasPlugin(VersionedPluginId plugin) => _pluginDetails.ContainsKey(plugin);

    public Task<MayError<IInstalledPlugin>> InstallPlugin(
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost,
        CancellationToken cancellationToken = default)
    {
        return installer.InstallFromArchive(File.OpenRead(_pluginDetails[pluginId].PackagePath), pluginId, runtimeHost);
    }

    public Task<ManifestData> GetManifest(VersionedPluginId plugin, CancellationToken cancellationToken = default)
        => Task.FromResult(_pluginDetails[plugin].Manifest);

    private record RepositoryPluginDetails(ManifestData Manifest, FileSystemPath PackagePath);
}