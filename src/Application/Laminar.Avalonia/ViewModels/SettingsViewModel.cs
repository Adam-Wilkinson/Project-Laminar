using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.PluginFramework;

namespace Laminar.Avalonia.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IRuntimeHostManager _runtimeHostManager;

    public SettingsViewModel(IPluginLibrary pluginLibrary,
        IRuntimeHostManager runtimeHostManager,
        IExceptionHandler exceptionHandler)
    {
        _runtimeHostManager = runtimeHostManager;
        AvailablePlugins = pluginLibrary.LoadedPlugins
            .ObservableMap(plugin => new PluginInfoViewModel(plugin, exceptionHandler, runtimeHostManager));
        PluginSources = pluginLibrary.Sources;
        Runtimes = runtimeHostManager.AllHosts;
        runtimeHostManager.PluginsChanged += OnPluginsChanged;
    }

    private void OnPluginsChanged(object? sender, PluginsChangedEventArgs e)
    {
        OnPropertyChanged(nameof(InstalledPlugins));
    }

    public string PluginFrameworkVersion => PluginFrameworkInfo.Version;

    public IReadOnlyObservableCollection<PluginInfoViewModel> AvailablePlugins { get; }

    public IReadOnlyList<IPluginSource> PluginSources { get; }

    public IReadOnlyCollection<IRuntimeHost> Runtimes { get; }

    public IEnumerable<IInstalledPlugin> InstalledPlugins =>
        _runtimeHostManager.AllHosts.SelectMany(x => x.PluginManager.Plugins);
}