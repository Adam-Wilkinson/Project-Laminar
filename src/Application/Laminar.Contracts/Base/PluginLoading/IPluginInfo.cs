using System.Diagnostics.CodeAnalysis;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInfo
{
    public string Id { get; }

    public IReadOnlyObservableCollection<SemanticVersion> AllVersions { get; }

    public IReadOnlyObservableValue<SemanticVersion?> LatestVersion { get; }

    public bool HasVersion(SemanticVersion version, [NotNullWhen(true)] out IList<IPluginSource>? sources);
    
    public Task InstallVersion(SemanticVersion version, IRuntimeHost host);
}