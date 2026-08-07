namespace Laminar.Domain.ValueObjects;

public readonly struct VersionedPluginInfo(string version)
{
    public SemanticVersion Version { get; } = new(version);

    public required string Path { get; init; }
}

public class PluginInfo
{
    private readonly SortedSet<VersionedPluginInfo> _versions;

    public PluginInfo(string id, List<VersionedPluginInfo> allVersions)
    {
        _versions = new SortedSet<VersionedPluginInfo>(allVersions,
            Comparer<VersionedPluginInfo>.Create((a, b) => a.Version.CompareTo(b.Version)));
        Id = id;
        LatestVersion = _versions.Max;
    }

    public string Id { get; }
    
    public IReadOnlyCollection<VersionedPluginInfo> AllVersions => _versions;

    public VersionedPluginInfo LatestVersion { get; private set; }

    public void MergeFrom(PluginInfo pluginInfo)
    {
        foreach (var version in pluginInfo.AllVersions)
        {
            AddVersion(version);
        }
    }
    
    public void AddVersion(VersionedPluginInfo pluginInfo)
    {
        if (!_versions.Add(pluginInfo))
        {
            return;
        }

        LatestVersion = _versions.Max;
    }
}