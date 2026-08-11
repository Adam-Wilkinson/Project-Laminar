using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.PluginFramework;

namespace Laminar.Avalonia.ViewModels;
public partial class SettingsViewModel(
    IPluginRepositoryStore pluginRepositoryStore,
    IExceptionHandler exceptionHandler) : ViewModelBase
{
    public string PluginFrameworkVersion => PluginFrameworkInfo.Version;

    public IReadOnlyObservableCollection<PluginInfoViewModel> AvailablePlugins { get; }
        = pluginRepositoryStore.LoadedPlugins.ObservableMap(plugin => new PluginInfoViewModel(plugin, exceptionHandler));
}