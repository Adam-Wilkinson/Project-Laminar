using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Implementation.Base.PluginLoading;

internal class PluginInfo : IPluginInfo
{
    private readonly SortedList<SemanticVersion, (VersionedPluginId id, List<IPluginRepository> sources)> _versions = [];
    
    public PluginInfo(string id)
    {
        AllVersions = _versions.Keys.AsReadOnly();
        Id = id;
        LatestVersion = _versions.GetKeyAtIndex(_versions.Count - 1);
    }

    public string Id { get; }

    public IReadOnlyCollection<SemanticVersion> AllVersions { get; }

    public SemanticVersion LatestVersion { get; private set; }

    public Task<ManifestData> GetVersionInfo(SemanticVersion version, CancellationToken ct = default)
        => _versions[version].sources[0].GetManifest(new VersionedPluginId(Id, version), ct);

    public Task<Stream> OpenVersionStream(SemanticVersion version, CancellationToken ct = default)
        => _versions[version].sources[0].StreamPlugin(new VersionedPluginId(Id, version), ct);

    public bool HasVersion(SemanticVersion version) => _versions.ContainsKey(version);

    public void AddVersion(SemanticVersion version, IPluginRepository sourceRepository)
    {
        if (_versions.TryGetValue(version, out var currentVersionInfo))
        {
            currentVersionInfo.sources.Add(sourceRepository);
            return;
        }

        var newVersionInfo = new VersionedPluginId(Id, version);
        _versions.Add(version, (newVersionInfo, [sourceRepository]));
        LatestVersion = _versions.GetKeyAtIndex(_versions.Count - 1);
    }

    public void RemoveVersion(SemanticVersion version, IPluginRepository sourceRepository)
    {
        if (!_versions.TryGetValue(version, out var currentVersionInfo))
        {
            return;
        }
        
        currentVersionInfo.sources.Remove(sourceRepository);
        if (currentVersionInfo.sources.Count != 0) return;
        
        _versions.Remove(version);
        LatestVersion = _versions.GetKeyAtIndex(_versions.Count - 1);
    }
}