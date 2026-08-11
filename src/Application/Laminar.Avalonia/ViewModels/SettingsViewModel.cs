using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.PluginFramework;

namespace Laminar.Avalonia.ViewModels;
public partial class SettingsViewModel(IPluginRepositoryStore pluginRepositoryStore) : ViewModelBase
{
    public string PluginFrameworkVersion => PluginFrameworkInfo.Version;

    public IReadOnlyObservableCollection<IPluginInfo> Plugins { get; } = pluginRepositoryStore.LoadedPlugins;
}