using System.ComponentModel;
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
    private readonly IExceptionHandler _exceptionHandler;
    private readonly Dictionary<string, PluginInfoViewModel> _availablePlugins = [];
    private readonly IPluginLibrary _pluginLibrary;
    
    public SettingsViewModel(IPluginLibrary pluginLibrary,
        IWorkspaceManager workspaceManager,
        IExceptionHandler exceptionHandler)
    {
        _workspaceManager = workspaceManager;
        _exceptionHandler = exceptionHandler;
        _pluginLibrary = pluginLibrary;
        AvailablePlugins = pluginLibrary.LoadedPlugins.ObservableMap(PluginInfoFactory);
        PluginSources = pluginLibrary.Sources;
        Workspaces = workspaceManager.Workspaces;
        workspaceManager.PluginChanged += OnPluginChanged;
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
    
    private PluginInfoViewModel PluginInfoFactory(IPluginInfo plugin)
    {
        var newPlugin = new PluginInfoViewModel(plugin, _exceptionHandler, _workspaceManager);
        newPlugin.PropertyChanged += OnPluginPropertyChanged;
        if (newPlugin.IsInstalled)
        {
            InstalledPlugins.Add(newPlugin);
        }

        _availablePlugins.Add(plugin.Id, newPlugin);
        return newPlugin;
    }

    private void OnPluginPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not PluginInfoViewModel changedPlugin 
            || e.PropertyName != nameof(PluginInfoViewModel.IsInstalled)) return;
        
        switch (changedPlugin.IsInstalled)
        {
            case true when !InstalledPlugins.Contains(changedPlugin):
                InstalledPlugins.Add(changedPlugin);
                break;
            case false when InstalledPlugins.Contains(changedPlugin):
                InstalledPlugins.Remove(changedPlugin);
                break;
        }
    }

    private void OnPluginChanged(object? sender, PluginChangedEventArgs e) 
        => _availablePlugins[e.PluginId].OnInstalledVersionsChanged();
}