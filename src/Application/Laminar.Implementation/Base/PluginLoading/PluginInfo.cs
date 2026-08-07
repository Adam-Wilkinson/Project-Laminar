using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base.PluginLoading;

internal class PluginInfo : IPluginInfo
{
    private readonly SortedList<SemanticVersion, VersionedPluginInfo> _versions;
    
    public PluginInfo(string id, List<VersionedPluginInfo> allVersions)
    {
        _versions = new SortedList<SemanticVersion, VersionedPluginInfo>(
            allVersions.ToDictionary(x => x.Version), SemanticVersionComparer.Instance);
        AllVersions = _versions.Values.AsReadOnly();
        Id = id;
        LatestVersion = _versions.GetValueAtIndex(_versions.Count - 1);
    }

    public string Id { get; }

    public IReadOnlyCollection<VersionedPluginInfo> AllVersions { get; }

    public VersionedPluginInfo LatestVersion { get; private set; }

    public bool TryGetVersion(SemanticVersion version, out VersionedPluginInfo versionedPluginInfo)
        => _versions.TryGetValue(version, out versionedPluginInfo);

    public void AddVersion(VersionedPluginInfo pluginInfo, IPluginRepository sourceRepository)
    {
        if (_versions.TryGetValue(pluginInfo.Version, out var currentVersionInfo))
        {
            currentVersionInfo.Sources.Add(sourceRepository);
            return;
        }

        _versions.Add(pluginInfo.Version, pluginInfo);
        LatestVersion = _versions.GetValueAtIndex(_versions.Count - 1);
    }

    public void RemoveVersion(VersionedPluginInfo pluginInfo, IPluginRepository sourceRepository)
    {
        if (!_versions.TryGetValue(pluginInfo.Version, out var currentVersionInfo))
        {
            return;
        }
        
        currentVersionInfo.Sources.Remove(sourceRepository);
        if (currentVersionInfo.Sources.Count != 0) return;
        
        _versions.Remove(pluginInfo.Version);
        LatestVersion = _versions.GetValueAtIndex(_versions.Count - 1);
    }
}