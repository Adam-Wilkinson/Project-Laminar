using System.Collections.ObjectModel;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base.PluginLoading;

internal class PluginInfo(string id) : IPluginInfo
{
    private readonly SortedList<SemanticVersion, (VersionedPluginId id, List<IPluginSource> sources)> _versions = [];
    private readonly ObservableCollection<SemanticVersion> _allVersions = [];
    private readonly ObservableValue<SemanticVersion?> _latestVersion = new(null);
    
    public string Id { get; } = id;

    public IReadOnlyObservableCollection<SemanticVersion> AllVersions => field ??= _allVersions.ToInterfaceImpl();

    public IReadOnlyObservableValue<SemanticVersion?> LatestVersion => _latestVersion;

    public bool HasVersion(SemanticVersion version) => _versions.ContainsKey(version);

    public void AddVersion(SemanticVersion version, IPluginSource sourceSource)
    {
        if (_versions.TryGetValue(version, out var currentVersionInfo))
        {
            currentVersionInfo.sources.Add(sourceSource);
            return;
        }

        var newVersionInfo = new VersionedPluginId(Id, version);
        _versions.Add(version, (newVersionInfo, [sourceSource]));
        _allVersions.Insert(_versions.IndexOfKey(version), version);
        _latestVersion.Value = _versions.GetKeyAtIndex(_versions.Count - 1);
    }

    public void RemoveVersion(SemanticVersion version, IPluginSource sourceSource)
    {
        if (!_versions.TryGetValue(version, out var currentVersionInfo))
        {
            return;
        }
        
        currentVersionInfo.sources.Remove(sourceSource);
        if (currentVersionInfo.sources.Count != 0) return;
        
        var versionIndex = _versions.IndexOfKey(version);
        _versions.Remove(version);
        _allVersions.RemoveAt(versionIndex);
        _latestVersion.Value = _versions.GetKeyAtIndex(_versions.Count - 1);
    }
}