using System.Collections.ObjectModel;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Implementation.Base.PluginLoading;

internal class PluginInfo(string id) : IPluginInfo
{
    private readonly SortedList<SemanticVersion, (VersionedPluginId id, List<IPluginRepository> sources)> _versions = [];
    private readonly ObservableCollection<SemanticVersion> _allVersions = [];
    private readonly ObservableValue<SemanticVersion?> _latestVersion = new(null);
    
    public string Id { get; } = id;

    public IReadOnlyObservableCollection<SemanticVersion> AllVersions => field ??= _allVersions.ToInterfaceImpl();

    public IReadOnlyObservableValue<SemanticVersion?> LatestVersion => _latestVersion;

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
        _allVersions.Insert(_versions.IndexOfKey(version), version);
        _latestVersion.Value = _versions.GetKeyAtIndex(_versions.Count - 1);
    }

    public void RemoveVersion(SemanticVersion version, IPluginRepository sourceRepository)
    {
        if (!_versions.TryGetValue(version, out var currentVersionInfo))
        {
            return;
        }
        
        currentVersionInfo.sources.Remove(sourceRepository);
        if (currentVersionInfo.sources.Count != 0) return;
        
        var versionIndex = _versions.IndexOfKey(version);
        _versions.Remove(version);
        _allVersions.RemoveAt(versionIndex);
        _latestVersion.Value = _versions.GetKeyAtIndex(_versions.Count - 1);
    }
}