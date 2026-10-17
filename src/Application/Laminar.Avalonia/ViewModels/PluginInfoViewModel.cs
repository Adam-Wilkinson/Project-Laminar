using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.ViewModels.Contracts;
using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Avalonia.ViewModels;

public partial class PluginInfoViewModel : ViewModelBase, IVersioningHost, IChangeVersion
{
    private readonly IPluginInfo _pluginInfo;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly ObservableDictionary<IWorkspace, WorkspacePluginInfoViewModel> _workspacePluginInfos = [];
    
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
        TargetVersionName = info.Id;
        TargetVersion = info.LatestVersion.Value;
        if (TargetVersion is not null)
        {
            _ = SetTargetVersion(TargetVersion.Value);
        }

        _pluginInfo.LatestVersion.Changed += LatestVersionChanged;
        SetVersionType = ComputeChangeVersionType();
    }

    public IReadOnlyObservableList<SemanticVersion> AvailableVersions { get; }

    public IReadOnlyObservableBag<WorkspacePluginInfoViewModel> Workspaces => 
        field ??= _workspacePluginInfos.Values.ToObservableList(Comparer<WorkspacePluginInfoViewModel>
            .Create((a, b) => string.Compare(a.HostName, b.HostName, StringComparison.Ordinal)));
    
    [ObservableProperty] public partial RelativeVersionState SetVersionType { get; private set; }

    [ObservableProperty] public partial SemanticVersion? TargetVersion { get; set; }

    [ObservableProperty] public partial string TargetVersionName { get; set; }

    [ObservableProperty] public partial string Description { get; set; } = "";
    
    [ObservableProperty] public partial bool IsExpanded { get; set; }

    [ObservableProperty] public partial bool IsInstalled { get; set; }
    
    public void OnInstalledVersionsChanged()
    {
        EnsureWorkspaceInfosLoaded();
        IsInstalled = _workspacePluginInfos.Values.Any(x => x.InstalledVersion is not null);
    }
    
    partial void OnTargetVersionChanged(SemanticVersion? value)
    {
        if (value is not { } newVersion) return;

        _ = SetTargetVersion(newVersion); 
    }

    partial void OnIsExpandedChanged(bool value)
    {
        if (value) EnsureWorkspaceInfosLoaded();
    }

    protected override void OnDisposed()
    {
        _pluginInfo.LatestVersion.Changed -= LatestVersionChanged;
    }

    [RelayCommand]
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
            TargetVersionName = newVersionMetadata.UserFriendlyName.Localize(CultureInfo.CurrentUICulture) ?? _pluginInfo.Id;
            Description = newVersionMetadata.Description.Localize(CultureInfo.CurrentUICulture) ?? "";
            SetVersionType = ComputeChangeVersionType();
        }
        
        catch (Exception ex)
        {
            await _exceptionHandler.OnExceptionAsync(ex);
        }
    }

    private void LatestVersionChanged(object? sender, ObservableValueChangedEventArgs<SemanticVersion?> changedArgs)
    {
        if (Equals(changedArgs.OldValue, TargetVersion))
        {
            TargetVersion = changedArgs.NewValue;
        }
    }

    public void EnsureWorkspaceInfosLoaded()
    {
        if (_workspaceInfosLoaded) return;
        _workspaceInfosLoaded = true;
        
        RegisterSubscription(_workspaceManager.Workspaces.SubscribeForEach(
            workspace =>
            {
                _workspacePluginInfos.Add(workspace, new WorkspacePluginInfoViewModel(_pluginInfo.Id, this, workspace));
            },
            workspace =>
            {
                _workspacePluginInfos.Remove(workspace);
            }));
    }

    private RelativeVersionState ComputeChangeVersionType()
    {
        var hasInstallations = false;
        var hasUpgrades = false;
        var hasDowngrades = false;
        foreach (var workspacePluginInfo in Workspaces)
        {
            if (workspacePluginInfo.InstalledVersion is not { } installedVersion)
                continue;
            
            hasInstallations = true;
            hasUpgrades &= installedVersion < TargetVersion;
            hasDowngrades &= installedVersion > TargetVersion;
        }

        if (!hasInstallations) return RelativeVersionState.Install;
        return (hasUpgrades, hasDowngrades) switch
        {
            (false, false) => RelativeVersionState.Equal,
            (true, false) => RelativeVersionState.Upgrade,
            (false, true) => RelativeVersionState.Downgrade,
            (true, true) => RelativeVersionState.SetVersion
        };
    }
}