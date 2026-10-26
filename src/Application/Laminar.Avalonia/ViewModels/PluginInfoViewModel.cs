using System.ComponentModel;
using System.Globalization;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.Translations;
using Laminar.Avalonia.ViewModels.Contracts;
using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Avalonia.ViewModels;

public partial class PluginInfoViewModel : ViewModelBase, IVersioningHost, ISetVersion, IUninstallable
{
    private static readonly IComparer<WorkspacePluginInfoViewModel> WorkspaceComparer =
        Comparer<WorkspacePluginInfoViewModel>.Create((a, b)
            => string.Compare(a.HostName, b.HostName, StringComparison.Ordinal));
    
    private readonly IPluginInfo _pluginInfo;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly ObservableDictionary<IWorkspace, WorkspacePluginInfoViewModel> _workspacePluginInfos = [];
    private readonly ObservableValue<string> _targetVersionName;
    private readonly ObservableValue<SemanticVersion?> _targetVersion;
    
    private bool _workspaceInfosLoaded = false;
    
    public PluginInfoViewModel(
        IPluginInfo info, 
        IExceptionHandler exceptionHandler,
        IWorkspaceManager workspaceManager)
    {
        _pluginInfo = info;
        _exceptionHandler = exceptionHandler;
        _workspaceManager = workspaceManager;

        AvailableVersions = info.AllVersions;
        _targetVersionName = new(info.Id);
        _targetVersion = new(info.LatestVersion.Value);
        RegisterSubscription(_targetVersion.Subscribe(x =>
        {
            if (x is not { } newVersion) return;
            _ = SetTargetVersion(newVersion);
        }));

        _pluginInfo.LatestVersion.Changed += LatestVersionChanged;
        SetVersionType = ComputeChangeVersionType();
    }

    public IReadOnlyObservableList<SemanticVersion> AvailableVersions { get; }

    public IReadOnlyObservableBag<WorkspacePluginInfoViewModel> Workspaces => 
        field ??= _workspacePluginInfos.Values.ToObservableList(WorkspaceComparer);

    public IObservable<string> SetVersionDescription => 
        field ??= Strings.SetPluginVersion.Value.CombineLatest(_targetVersionName, _targetVersion)
        .Select(values => string.Format(values.First, values.Second, values.Third));

    [ObservableProperty] public partial RelativeVersionState SetVersionType { get; private set; }

    public IObservableValue<SemanticVersion?> TargetVersion => _targetVersion;

    public IReadOnlyObservableValue<string> TargetVersionName => _targetVersionName;

    [ObservableProperty] public partial SemanticVersion? MinimumInstalledVersion { get; private set; }

    [ObservableProperty] public partial string Description { get; set; } = "";
    
    [ObservableProperty] public partial bool IsExpanded { get; set; }
    
    public bool CanSetVersion => SetVersionType != RelativeVersionState.Equal;
    
    public void OnInstalledVersionsChanged()
    {
        EnsureWorkspaceInfosLoaded();
        SemanticVersion? minimumVersion = null;
        foreach (var workspacePluginInfo in _workspacePluginInfos.Values)
        {
            if (workspacePluginInfo.InstalledVersion is not { } installed)
                continue;

            if (minimumVersion is null || installed < minimumVersion.Value)
            {
                minimumVersion = installed;
            }
        }

        MinimumInstalledVersion = minimumVersion;
        SetVersionType = ComputeChangeVersionType();
    }

    partial void OnSetVersionTypeChanged(RelativeVersionState value) => SetVersionCommand.NotifyCanExecuteChanged();

    partial void OnIsExpandedChanged(bool value)
    {
        if (value) EnsureWorkspaceInfosLoaded();
    }

    protected override void OnDisposed()
    {
        _pluginInfo.LatestVersion.Changed -= LatestVersionChanged;
    }

    [RelayCommand(CanExecute = nameof(CanSetVersion))]
    private async Task SetVersion()
    {
        EnsureWorkspaceInfosLoaded();
        foreach (var workspacePluginInfo in _workspacePluginInfos.Values)
        {
            if (workspacePluginInfo.InstalledVersion is not null || SetVersionType == RelativeVersionState.Install)
            {
                await workspacePluginInfo.SetVersion();
            }
        }
    }
    
    [RelayCommand]
    private void Uninstall()
    {
        foreach (var workspacePluginInfo in _workspacePluginInfos.Values)
        {
            workspacePluginInfo.Uninstall();
        }
    }

    private async Task SetTargetVersion(SemanticVersion newVersion)
    {
        try
        {
            if (!_pluginInfo.HasVersion(newVersion, out var sources)
                || sources.Count == 0)
            {
                throw new InvalidOperationException("Unable to find plugin version");
            }
            
            var newVersionMetadata = await sources[0].GetManifest(new VersionedPluginId(_pluginInfo.Id, newVersion));
            _targetVersionName.Value = newVersionMetadata.UserFriendlyName.Localize(CultureInfo.CurrentUICulture) ?? _pluginInfo.Id;
            Description = newVersionMetadata.Description.Localize(CultureInfo.CurrentUICulture) ?? "";
        }
        
        catch (Exception ex)
        {
            await _exceptionHandler.OnExceptionAsync(ex);
        }
    }

    private void LatestVersionChanged(object? sender, ObservableValueChangedEventArgs<SemanticVersion?> changedArgs) 
        => TargetVersion.Value = changedArgs.NewValue;

    private void EnsureWorkspaceInfosLoaded()
    {
        if (_workspaceInfosLoaded) return;
        _workspaceInfosLoaded = true;
        
        RegisterSubscription(_workspaceManager.Workspaces.SubscribeForEach(
            workspace =>
            {
                var newWorkspaceInfo = new WorkspacePluginInfoViewModel(_pluginInfo.Id, this, workspace);
                _workspacePluginInfos.Add(workspace, newWorkspaceInfo);
                newWorkspaceInfo.PropertyChanged += OnWorkspacePropertyChanged;
            },
            workspace =>
            {
                if (_workspacePluginInfos.Remove(workspace, out var workspaceInfo))
                {
                    workspaceInfo.PropertyChanged -= OnWorkspacePropertyChanged;
                }
            }));
    }

    private void OnWorkspacePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WorkspacePluginInfoViewModel.SetVersionType))
        {
            SetVersionType = ComputeChangeVersionType();
        }
    }

    private RelativeVersionState ComputeChangeVersionType() => MinimumInstalledVersion is null
        ? RelativeVersionState.Install
        : Workspaces.Select(x => x.SetVersionType).Aggregate();
}