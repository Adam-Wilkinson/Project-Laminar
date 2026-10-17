using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.ViewModels.Contracts;
using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels;

public partial class WorkspacePluginInfoViewModel : ViewModelBase, IChangeVersion
{
    private readonly IReadOnlyObservableValue<SemanticVersion?> _versionObservable;

    private readonly string _pluginId;
    private readonly IVersioningHost _versioningHost;
    private readonly IWorkspace _host;

    public WorkspacePluginInfoViewModel(
        string pluginId, 
        IVersioningHost versioningHost,
        IWorkspace host) : base(versioningHost)
    {
        _pluginId = pluginId;
        _versioningHost = versioningHost;
        _host = host;
        _versionObservable = host.PluginManager.GetPluginVersionObservable(pluginId);
        RegisterSubscription(_versionObservable.Subscribe(_ =>
        {
            OnPropertyChanged(nameof(InstalledVersion));
            SetVersionType = InstalledVersion.RelativeChangeTo(TargetVersion);
            SetVersionCommand.NotifyCanExecuteChanged();
            _versioningHost.OnInstalledVersionsChanged();
        })); 
        
        HostName = host.Name;
        TargetVersionName = versioningHost.TargetVersionName;
        TargetVersion = versioningHost.TargetVersion;
    }

    public string HostName { get; }

    [ObservableProperty] public partial string TargetVersionName { get; private set; }
    
    [ObservableProperty] public partial RelativeVersionState SetVersionType { get; private set; }

    [ObservableProperty] public partial SemanticVersion? TargetVersion { get; private set; }

    public bool CanSetVersion => TargetVersion is not null && !Equals(TargetVersion, InstalledVersion);

    public SemanticVersion? InstalledVersion => _versionObservable.Value;

    protected override void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        TargetVersionName = _versioningHost.TargetVersionName;
        TargetVersion = _versioningHost.TargetVersion;
    }

    partial void OnTargetVersionChanged(SemanticVersion? value)
    {
        if (value is null) return;
        SetVersionType = InstalledVersion.RelativeChangeTo(value);
        SetVersionCommand.NotifyCanExecuteChanged();
    }
    
    [RelayCommand(CanExecute = nameof(CanSetVersion))]
    public async Task SetVersion()
    {
        if (Equals(InstalledVersion, TargetVersion))
        {
            return;
        }

        if (InstalledVersion is not null)
        {
            _host.PluginManager.UninstallPlugin(_pluginId);
        }

        if (TargetVersion is not null)
        {
            await _host.PluginManager.EnsurePluginInstalled(new VersionedPluginId(_pluginId, TargetVersion.Value));
        }
        
        SetVersionCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(InstalledVersion));
    }

    [RelayCommand]
    public void Uninstall()
    {
        _host.PluginManager.UninstallPlugin(_pluginId);
        SetVersionCommand.NotifyCanExecuteChanged();
    }
}