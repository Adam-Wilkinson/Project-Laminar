using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Workspaces.PluginLoading;

internal class PluginInfo(string id) : IPluginInfo
{
    private static readonly Comparer<SemanticVersion> ReverseDirectionComparer =
        Comparer<SemanticVersion>.Create((x, y) => y.CompareTo(x)); 
    
    private readonly SortedList<SemanticVersion, IList<IPluginSource>> _versions = new(ReverseDirectionComparer);
    private readonly ObservableList<SemanticVersion> _allVersions = [];
    private readonly ObservableValue<SemanticVersion?> _latestVersion = new(null);
    
    public string Id { get; } = id;

    public IReadOnlyObservableList<SemanticVersion> AllVersions => _allVersions;

    public IReadOnlyObservableValue<SemanticVersion?> LatestVersion => _latestVersion;

    public bool HasVersion(SemanticVersion version, [NotNullWhen(true)] out IList<IPluginSource>? sources) 
        => _versions.TryGetValue(version, out sources);

    public void AddVersion(SemanticVersion version, IPluginSource source)
    {
        if (_versions.TryGetValue(version, out var sources))
        {
            sources.Add(source);
            return;
        }

        _versions.Add(version, [source]);
        _allVersions.Insert(_versions.IndexOfKey(version), version);
        _latestVersion.Value = _versions.GetKeyAtIndex(0);
    }

    public void RemoveVersion(SemanticVersion version, IPluginSource sourceSource)
    {
        if (!_versions.TryGetValue(version, out var sources))
        {
            return;
        }
        
        sources.Remove(sourceSource);
        if (sources.Count != 0) return;
        
        var versionIndex = _versions.IndexOfKey(version);
        _versions.Remove(version);
        _allVersions.RemoveAt(versionIndex);
        _latestVersion.Value = _versions.GetKeyAtIndex(0);
    }
}