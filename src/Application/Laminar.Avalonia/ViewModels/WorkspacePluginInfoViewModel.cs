using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.Translations;
using Laminar.Avalonia.ViewModels.Contracts;
using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.Contracts.Workspaces;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels;

public partial class WorkspacePluginInfoViewModel : ViewModelBase, IUninstallable, ISetVersion
{
    private readonly IReadOnlyObservableValue<SemanticVersion?> _installedVersionObservable;
    private readonly string _pluginId;
    private readonly IVersioningHost _versioningHost;
    private readonly IWorkspace _host;

    public WorkspacePluginInfoViewModel(
        string pluginId, 
        IVersioningHost versioningHost,
        IWorkspace host)
    {
        _pluginId = pluginId;
        _versioningHost = versioningHost;
        _host = host;
        _installedVersionObservable = host.PluginManager.GetPluginVersionObservable(pluginId);
        
        RegisterSubscription(_installedVersionObservable.Subscribe(_ =>
        {
            OnPropertyChanged(nameof(InstalledVersion));
            _versioningHost.OnInstalledVersionsChanged();
            SetVersionType = InstalledVersion.RelativeChangeTo(_versioningHost.TargetVersion.Value);
        }));

        RegisterSubscription(_versioningHost.TargetVersion.Subscribe(
            x => SetVersionType = _installedVersionObservable.Value.RelativeChangeTo(x)));
        
        HostName = host.Name;
        SetVersionDescription = Strings.SetPluginVersion.Value.CombineLatest(_versioningHost.TargetVersionName, _versioningHost.TargetVersion)
            .Select(values => string.Format(values.First, values.Second, values.Third));
    }

    public string HostName { get; }

    [ObservableProperty] public partial RelativeVersionState SetVersionType { get; private set; }

    public bool CanSetVersion => SetVersionType is not RelativeVersionState.Equal;

    public SemanticVersion? InstalledVersion => _installedVersionObservable.Value;

    public IObservable<string> SetVersionDescription { get; }

    partial void OnSetVersionTypeChanged(RelativeVersionState value) => SetVersionCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanSetVersion))]
    public async Task SetVersion()
    {
        if (Equals(InstalledVersion, _versioningHost.TargetVersion.Value))
            return;

        if (_versioningHost.TargetVersion.Value is not { } targetVersion)
        {
            await _host.PluginManager.UninstallPlugin(_pluginId);
        }
        else
        {
            await _host.PluginManager.EnsurePluginInstalled(new VersionedPluginId(_pluginId, targetVersion));
        }
        
        SetVersionCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(InstalledVersion));
    }

    [RelayCommand]
    public async Task Uninstall()
    {
        await _host.PluginManager.UninstallPlugin(_pluginId);
        SetVersionCommand.NotifyCanExecuteChanged();
    }
}