using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.PluginFramework;

namespace Laminar.Avalonia.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IRuntimeHostManager _runtimeHostManager;
    private readonly Dictionary<string, PluginInfoViewModel> _availablePlugins = [];
    private readonly IPluginLibrary _pluginLibrary;
    
    public SettingsViewModel(IPluginLibrary pluginLibrary,
        IRuntimeHostManager runtimeHostManager,
        IExceptionHandler exceptionHandler)
    {
        _runtimeHostManager = runtimeHostManager;
        _pluginLibrary = pluginLibrary;
        AvailablePlugins = pluginLibrary.LoadedPlugins
            .ObservableMap(plugin =>
            {
                var newPlugin = new PluginInfoViewModel(plugin, exceptionHandler, runtimeHostManager);
                _availablePlugins.Add(plugin.Id, newPlugin);
                return newPlugin;
            });
        PluginSources = pluginLibrary.Sources;
        Runtimes = runtimeHostManager.AllHosts;
        runtimeHostManager.PluginsChanged += OnPluginsChanged;
    }

    private void OnPluginsChanged(object? sender, PluginsChangedEventArgs e)
    {
        var changedPlugin = _availablePlugins[e.PluginId];
        
        switch (e.ChangeType)
        {
            case PluginChangedType.Added:
                changedPlugin.Installations.Add(e.ChangedRuntime);
                break;
            case PluginChangedType.Removed:
                changedPlugin.Installations.Remove(e.ChangedRuntime);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (changedPlugin.Installations.Count)
        {
            case > 0 when !InstalledPlugins.Contains(changedPlugin):
                InstalledPlugins.Add(changedPlugin);
                break;
            case 0 when InstalledPlugins.Contains(changedPlugin):
                InstalledPlugins.Remove(changedPlugin);
                break;
        }
    }

    public string PluginFrameworkVersion => PluginFrameworkInfo.Version;

    public IReadOnlyObservableList<PluginInfoViewModel> AvailablePlugins { get; }

    public IReadOnlyList<IPluginSource> PluginSources { get; }

    public IReadOnlyCollection<IRuntimeHost> Runtimes { get; }

    public ObservableList<PluginInfoViewModel> InstalledPlugins { get; } = [];

    protected override void OnDisposed()
    {
        _runtimeHostManager.PluginsChanged -= OnPluginsChanged;
    }
}