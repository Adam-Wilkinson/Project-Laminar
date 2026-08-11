using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
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
    
    public PluginInfoViewModel(IPluginInfo info, IExceptionHandler exceptionHandler)
    {
        _pluginInfo = info;
        _exceptionHandler = exceptionHandler;
        
        AvailableVersions = info.AllVersions;
        Name = info.Id;
        SelectedVersion = info.LatestVersion.Value;
        if (SelectedVersion != null)
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

    private async Task SetNewVersion(SemanticVersion newVersion)
    {
        try
        {
            var newVersionMetadata = await _pluginInfo.GetVersionInfo(newVersion);
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