using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.PluginFramework;

namespace Laminar.Avalonia.ViewModels;
public partial class SettingsViewModel(
    IPluginLibrary pluginLibrary,
    IRuntimeHostManager runtimeHostManager,
    IExceptionHandler exceptionHandler) : ViewModelBase
{
    public string PluginFrameworkVersion => PluginFrameworkInfo.Version;

    public IReadOnlyObservableCollection<PluginInfoViewModel> AvailablePlugins { get; }
        = pluginLibrary.LoadedPlugins.ObservableMap(plugin => new PluginInfoViewModel(plugin, exceptionHandler));

    public IReadOnlyList<IPluginSource> PluginSources { get; }
        = pluginLibrary.Sources;

    public IReadOnlyCollection<IRuntimeHost> Runtimes { get; }
        = runtimeHostManager.AllHosts;

    public IEnumerable<IInstalledPlugin> InstalledPlugins =>
        runtimeHostManager.AllHosts.SelectMany(x => x.PluginManager.Plugins);
}