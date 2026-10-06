using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.PluginFramework;

namespace Laminar.Avalonia.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IRuntimeHostManager _runtimeHostManager;
    private readonly IPluginLibrary _pluginLibrary;
    
    public SettingsViewModel(IPluginLibrary pluginLibrary,
        IRuntimeHostManager runtimeHostManager,
        IExceptionHandler exceptionHandler)
    {
        _runtimeHostManager = runtimeHostManager;
        _pluginLibrary = pluginLibrary;
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

    public IReadOnlyObservableList<PluginInfoViewModel> AvailablePlugins { get; }

    public IReadOnlyList<IPluginSource> PluginSources { get; }

    public IReadOnlyCollection<IRuntimeHost> Runtimes { get; }

    public IEnumerable<PluginInfoViewModel> InstalledPlugins => [];

    protected override void OnDisposed()
    {
        _runtimeHostManager.PluginsChanged -= OnPluginsChanged;
    }
}