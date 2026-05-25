using System.Reflection;

namespace Laminar.Avalonia.ViewModels;
public partial class SettingsViewModel : ViewModelBase
{
    public string? PluginFrameworkVersion { get; } = typeof(App)
        .Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .FirstOrDefault(a => a.Key == "PluginFrameworkVersion")
        ?.Value?.Trim();
}