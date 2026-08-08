using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Implementation.Base.PluginLoading;

internal class PluginInfo : IPluginInfo
{
    private readonly SortedList<SemanticVersion, VersionedPluginInfo> _versions;
    
    public PluginInfo(string id, List<VersionedPluginInfo> allVersions)
    {
        _versions = new SortedList<SemanticVersion, VersionedPluginInfo>(
            allVersions.ToDictionary(x => x.Version), SemanticVersionComparer.Instance);
        AllVersions = _versions.Keys.AsReadOnly();
        Id = id;
        LatestVersion = _versions.GetKeyAtIndex(_versions.Count - 1);
    }

    public string Id { get; }

    public IReadOnlyCollection<SemanticVersion> AllVersions { get; }

    public SemanticVersion LatestVersion { get; private set; }

    public Task<ManifestData> GetVersionInfo(SemanticVersion version, CancellationToken ct = default)
        => _versions[version].Sources[0].GetManifest(Id, version, ct);

    public Task<Stream> OpenVersionStream(SemanticVersion version, CancellationToken ct = default)
        => _versions[version].Sources[0].StreamPlugin(Id, version, ct);

    public bool HasVersion(SemanticVersion version) => _versions.ContainsKey(version);

    public void AddVersion(SemanticVersion version, IPluginRepository sourceRepository)
    {
        if (_versions.TryGetValue(version, out var currentVersionInfo))
        {
            currentVersionInfo.Sources.Add(sourceRepository);
            return;
        }

        var newVersionInfo = new VersionedPluginInfo(Id, version);
        newVersionInfo.Sources.Add(sourceRepository);
        _versions.Add(version, newVersionInfo);
        LatestVersion = _versions.GetKeyAtIndex(_versions.Count - 1);
    }

    public void RemoveVersion(SemanticVersion version, IPluginRepository sourceRepository)
    {
        if (!_versions.TryGetValue(version, out var currentVersionInfo))
        {
            return;
        }
        
        currentVersionInfo.Sources.Remove(sourceRepository);
        if (currentVersionInfo.Sources.Count != 0) return;
        
        _versions.Remove(version);
        LatestVersion = _versions.GetKeyAtIndex(_versions.Count - 1);
    }
}