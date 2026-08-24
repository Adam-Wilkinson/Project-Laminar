using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base.PluginLoading;

internal class PluginInfo(string id) : IPluginInfo
{
    private readonly SortedList<SemanticVersion, IList<IPluginSource>> _versions = [];
    private readonly ObservableCollection<SemanticVersion> _allVersions = [];
    private readonly ObservableValue<SemanticVersion?> _latestVersion = new(null);
    
    public string Id { get; } = id;

    public IReadOnlyObservableCollection<SemanticVersion> AllVersions => field ??= _allVersions.ToInterfaceImpl();

    public IReadOnlyObservableValue<SemanticVersion?> LatestVersion => _latestVersion;

    public bool HasVersion(SemanticVersion version, [NotNullWhen(true)] out IList<IPluginSource>? sources) 
        => _versions.TryGetValue(version, out sources);

    public Task InstallVersion(SemanticVersion version, IRuntimeHost host) 
        => host.PluginManager.EnsurePluginInstalled(new VersionedPluginId(Id, version));

    public void AddVersion(SemanticVersion version, IPluginSource source)
    {
        if (_versions.TryGetValue(version, out var sources))
        {
            sources.Add(source);
            return;
        }

        _versions.Add(version, [source]);
        _allVersions.Insert(_versions.IndexOfKey(version), version);
        _latestVersion.Value = _versions.GetKeyAtIndex(_versions.Count - 1);
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
        _latestVersion.Value = _versions.GetKeyAtIndex(_versions.Count - 1);
    }
}