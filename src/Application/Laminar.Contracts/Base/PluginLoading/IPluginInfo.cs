using System.Diagnostics.CodeAnalysis;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInfo
{
    public string Id { get; }

    public IReadOnlyCollection<VersionedPluginInfo> AllVersions { get; }

    public VersionedPluginInfo LatestVersion { get; }
    
    public bool TryGetVersion(SemanticVersion version, out VersionedPluginInfo versionedPluginInfo);
    
    public void AddVersion(VersionedPluginInfo pluginInfo, IPluginRepository sourceRepository);
    
    public void RemoveVersion(VersionedPluginInfo pluginInfo, IPluginRepository sourceRepository);
}

public readonly struct VersionedPluginInfo(string id, string version)
{
    public SemanticVersion Version { get; } = new(version);

    public string Id { get; } = id;

    public List<IPluginRepository> Sources { get; } = [];
}