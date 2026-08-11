using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInfo
{
    public string Id { get; }

    public IReadOnlyCollection<SemanticVersion> AllVersions { get; }

    public SemanticVersion? LatestVersion { get; }

    public Task<ManifestData> GetVersionInfo(SemanticVersion version, CancellationToken ct = default);
    
    public Task<Stream> OpenVersionStream(SemanticVersion version, CancellationToken ct = default);
    
    public bool HasVersion(SemanticVersion version);
    
    public void AddVersion(SemanticVersion version, IPluginRepository sourceRepository);
    
    public void RemoveVersion(SemanticVersion version, IPluginRepository sourceRepository);
}