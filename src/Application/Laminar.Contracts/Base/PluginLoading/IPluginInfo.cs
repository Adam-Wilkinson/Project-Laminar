using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInfo
{
    public string Id { get; }

    public IReadOnlyObservableCollection<SemanticVersion> AllVersions { get; }

    public IReadOnlyObservableValue<SemanticVersion?> LatestVersion { get; }

    public bool HasVersion(SemanticVersion version);
    
    public void AddVersion(SemanticVersion version, IPluginSource sourceSource);
    
    public void RemoveVersion(SemanticVersion version, IPluginSource sourceSource);
}