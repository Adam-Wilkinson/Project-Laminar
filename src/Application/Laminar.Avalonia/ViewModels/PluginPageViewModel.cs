using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.Translations;
using Laminar.Avalonia.ViewModels.Contracts;
using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base;
using Laminar.Contracts.Workspaces;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;

namespace Laminar.Avalonia.ViewModels;

public partial class PluginPageViewModel : ViewModelBase, ISetVersion
{
    private readonly IWorkspaceManager _workspaceManager;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly Dictionary<string, PluginInfoViewModel> _availablePlugins = [];
    private readonly DialogService _dialogService;
    
    public PluginPageViewModel(
        IPluginLibrary pluginLibrary,
        IWorkspaceManager workspaceManager,
        IExceptionHandler exceptionHandler,
        DialogService dialogService)
    {
        _dialogService = dialogService;
        _workspaceManager = workspaceManager;
        _exceptionHandler = exceptionHandler;
        AvailablePlugins = pluginLibrary.LoadedPlugins.ObservableMap(PluginInfoFactory);
        PluginSources = pluginLibrary.Sources;
        workspaceManager.PluginChanged += OnPluginChanged;
    }
    
    public IReadOnlyObservableList<PluginInfoViewModel> AvailablePlugins { get; }

    public IReadOnlyList<IPluginSource> PluginSources { get; }
    
    public ObservableList<PluginInfoViewModel> InstalledPlugins { get; } = [];

    public bool CanChangeVersion => SetVersionType is not RelativeVersionState.Equal;

    [ObservableProperty] 
    public partial bool SourcesShown { get; set; }

    [ObservableProperty] 
    public partial RelativeVersionState SetVersionType { get; private set; }

    [ObservableProperty]
    public partial IObservable<string> SetVersionDescription { get; private set; } 
        = new ObservableValue<string>("All plugins up to date");

    [RelayCommand]
    private async Task Refresh() => 
        await _dialogService.PromptError("Not implemented", "Refreshing plugin page is not yet supported");

    [RelayCommand(CanExecute = nameof(CanChangeVersion))]
    private async Task SetVersion()
    {
        foreach (var installedPlugin in InstalledPlugins)
        {
            await installedPlugin.SetVersionCommand.ExecuteAsync(null);
        }
    }
    
    partial void OnSetVersionTypeChanged(RelativeVersionState value)
    {
        SetVersionCommand.NotifyCanExecuteChanged();
        SetVersionDescription = SetVersionType switch
        {
            RelativeVersionState.Equal => Strings.AllPluginsUpToDate.Value,
            RelativeVersionState.Upgrade => Strings.UpgradeAllPlugins.Value,
            _ => Strings.SetAllPlugins.Value,
        };
    }

    protected override void OnDisposed()
    {
        _workspaceManager.PluginChanged -= OnPluginChanged;
    }
    
    private PluginInfoViewModel PluginInfoFactory(IPluginInfo plugin)
    {
        var newPlugin = new PluginInfoViewModel(plugin, _exceptionHandler, _workspaceManager);
        newPlugin.PropertyChanged += OnPluginPropertyChanged;
        if (newPlugin.MinimumInstalledVersion is not null)
        {
            InstalledPlugins.Add(newPlugin);
        }

        _availablePlugins.Add(plugin.Id, newPlugin);
        return newPlugin;
    }

    private void OnPluginPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not PluginInfoViewModel changedPlugin) return;

        switch (e.PropertyName)
        {
            case nameof(PluginInfoViewModel.MinimumInstalledVersion):
                OnMinimumVersionChanged(changedPlugin);                
                break;
            case nameof(PluginInfoViewModel.SetVersionType):
                SetVersionType = InstalledPlugins.Select(x => x.SetVersionType).Aggregate();
                break;
        }
    }

    private void OnMinimumVersionChanged(PluginInfoViewModel changedPlugin)
    {
        if (changedPlugin.MinimumInstalledVersion is not null && !InstalledPlugins.Contains(changedPlugin))
        {
            InstalledPlugins.Add(changedPlugin);
        }
        else if (changedPlugin.MinimumInstalledVersion is null && InstalledPlugins.Contains(changedPlugin))
        {
            InstalledPlugins.Remove(changedPlugin);
        }
    }

    private void OnPluginChanged(object? sender, PluginChangedEventArgs e)
    {
        _availablePlugins[e.PluginId].OnInstalledVersionsChanged();
    }
}