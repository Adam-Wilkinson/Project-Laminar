using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Workspaces.PluginLoading;

public interface IInstalledPluginCollection : IReadOnlyObservableBag<string>
{
    public event PluginChangedEventHandler? Changed;

    public IReadOnlyObservableValue<SemanticVersion?> GetPluginVersionObservable(string pluginId);

    public SemanticVersion? GetPluginVersion(string pluginId);
}