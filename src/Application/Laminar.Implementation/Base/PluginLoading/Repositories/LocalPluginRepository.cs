using System.IO.Compression;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Implementation.Base.PluginLoading.Repositories;

public class LocalPluginRepository(
    string id, 
    FileSystemPath path, 
    IFileSystem fileSystem,
    IExceptionHandler handler) : IPluginRepository
{
    private readonly List<VersionedPluginId> _plugins = [];
    private readonly Dictionary<VersionedPluginId, RepositoryPluginDetails> _pluginDetails = new();
    
    public string Id { get; } = id;

    public IReadOnlyList<VersionedPluginId> Plugins => _plugins;
    
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
            _plugins.Add(pluginId);
            _pluginDetails.Add(pluginId, new RepositoryPluginDetails(manifest, pluginFile));
            yield return pluginId;
        }
    }

    public Task<Stream> StreamPlugin(VersionedPluginId plugin, CancellationToken cancellationToken = default)
        => Task.FromResult<Stream>(File.OpenRead(_pluginDetails[plugin].PackagePath));

    public Task<ManifestData> GetManifest(VersionedPluginId plugin, CancellationToken cancellationToken = default)
        => Task.FromResult(_pluginDetails[plugin].Manifest);

    private record RepositoryPluginDetails(ManifestData Manifest, FileSystemPath PackagePath);
}