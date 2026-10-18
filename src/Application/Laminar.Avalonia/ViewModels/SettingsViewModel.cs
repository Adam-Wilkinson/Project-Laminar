using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.PluginFramework;

namespace Laminar.Avalonia.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IWorkspaceManager _workspaceManager;
    private readonly Dictionary<string, PluginInfoViewModel> _availablePlugins = [];
    private readonly IPluginLibrary _pluginLibrary;
    
    public SettingsViewModel(IPluginLibrary pluginLibrary,
        IWorkspaceManager workspaceManager,
        IExceptionHandler exceptionHandler)
    {
        _workspaceManager = workspaceManager;
        _pluginLibrary = pluginLibrary;
        AvailablePlugins = pluginLibrary.LoadedPlugins
            .ObservableMap(plugin =>
            {
                var newPlugin = new PluginInfoViewModel(plugin, exceptionHandler, workspaceManager);
                _availablePlugins.Add(plugin.Id, newPlugin);
                return newPlugin;
            });
        PluginSources = pluginLibrary.Sources;
        Workspaces = workspaceManager.Workspaces;
        workspaceManager.PluginChanged += OnPluginChanged;
    }

    private void OnPluginChanged(object? sender, PluginChangedEventArgs e)
    {
        var changedPlugin = _availablePlugins[e.PluginId];
        changedPlugin.EnsureWorkspaceInfosLoaded();
        var installedPluginsContains = InstalledPlugins.Contains(changedPlugin);
        switch (changedPlugin.IsInstalled)
        {
            case true when installedPluginsContains:
                InstalledPlugins.Remove(changedPlugin);
                break;
            case false when !installedPluginsContains:
                InstalledPlugins.Add(changedPlugin);
                break;
        }
    }

    public string PluginFrameworkVersion => PluginFrameworkInfo.Version;

    public IReadOnlyObservableList<PluginInfoViewModel> AvailablePlugins { get; }

    public IReadOnlyList<IPluginSource> PluginSources { get; }

    public IReadOnlyCollection<IWorkspace> Workspaces { get; }

    public ObservableList<PluginInfoViewModel> InstalledPlugins { get; } = [];

    protected override void OnDisposed()
    {
        _workspaceManager.PluginChanged -= OnPluginChanged;
    }
}