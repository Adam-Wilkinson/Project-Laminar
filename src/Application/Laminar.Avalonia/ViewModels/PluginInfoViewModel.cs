using System.Globalization;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Avalonia.ViewModels;

public partial class PluginInfoViewModel : ViewModelBase
{
    private readonly IPluginInfo _pluginInfo;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IRuntimeHostManager _runtimeHostManager;

    public PluginInfoViewModel(
        IPluginInfo info, 
        IExceptionHandler exceptionHandler,
        IRuntimeHostManager runtimeHostManager)
    {
        _pluginInfo = info;
        _exceptionHandler = exceptionHandler;
        _runtimeHostManager = runtimeHostManager;

        AvailableVersions = info.AllVersions;
        Name = info.Id;
        SelectedVersion = info.LatestVersion.Value;
        if (SelectedVersion is not null)
        {
            _ = SetNewVersion(SelectedVersion.Value);
        }

        info.LatestVersion.OnChanged += (o, changedArgs) =>
        {
            if (Equals(changedArgs.OldValue, SelectedVersion))
            {
                SelectedVersion = changedArgs.NewValue;
            }
        };
    }

    public IReadOnlyObservableCollection<SemanticVersion> AvailableVersions { get; }
    
    [ObservableProperty] public partial SemanticVersion? SelectedVersion { get; set; }

    [ObservableProperty] public partial string Name { get; set; }

    [ObservableProperty] public partial string Description { get; set; } = "";

    partial void OnSelectedVersionChanged(SemanticVersion? value)
    {
        if (value is not { } newVersion) return;

        _ = SetNewVersion(newVersion); 
    }

    [RelayCommand]
    private async Task Install(IRuntimeHost target)
    {
        if (SelectedVersion is null)
        {
            await _exceptionHandler.OnExceptionAsync(new InvalidOperationException("Cannot install plugin without version"));
            return;
        }

        try
        {
            await target.PluginManager.EnsurePluginInstalled(new VersionedPluginId(_pluginInfo.Id,
                SelectedVersion.Value));
        }
        catch (Exception ex)
        {
            await _exceptionHandler.OnExceptionAsync(ex);
            
        }
    }

    [RelayCommand]
    private async Task InstallToAll()
    {
        foreach (var runtime in _runtimeHostManager.AllHosts)
        {
            await Install(runtime);
        }
    }

    private async Task SetNewVersion(SemanticVersion newVersion)
    {
        try
        {
            if (!_pluginInfo.HasVersion(newVersion, out var sources)
                || sources.Count == 0)
            {
                throw new InvalidOperationException("Unable to find plugin version");
            }
            
            var newVersionMetadata = await sources[0].GetManifest(new VersionedPluginId(_pluginInfo.Id, newVersion));
            Name = ExtractStringFrom(newVersionMetadata.UserFriendlyName) ?? _pluginInfo.Id;
            Description = ExtractStringFrom(newVersionMetadata.Description) ?? "";
        }
        catch (Exception ex)
        {
            await _exceptionHandler.OnExceptionAsync(ex);
        }
    }

    private static string? ExtractStringFrom(ManifestData.LangSupportedString langSupportedString)
    {
        if (langSupportedString.TryGetProperty(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, out var lang)
            && lang.ValueKind == JsonValueKind.String)
        {
            return (string)lang.AsString;
        }
        
        if (langSupportedString.TryGetString(out var str))
        {
            return str;
        }

        return null;
    }
}