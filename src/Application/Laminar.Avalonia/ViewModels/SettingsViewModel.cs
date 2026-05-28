using Laminar.PluginFramework;

namespace Laminar.Avalonia.ViewModels;
public partial class SettingsViewModel : ViewModelBase
{
    public string PluginFrameworkVersion => PluginFrameworkInfo.Version;
}