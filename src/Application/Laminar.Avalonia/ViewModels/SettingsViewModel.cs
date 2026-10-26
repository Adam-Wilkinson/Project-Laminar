using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.PluginFramework;

namespace Laminar.Avalonia.ViewModels;

public class SettingsViewModel(PluginPageViewModel pluginPage) : ViewModelBase
{
    public string PluginFrameworkVersion => PluginFrameworkInfo.Version;

    public PluginPageViewModel PluginPage => pluginPage;

    protected override void OnDisposed()
    {
        pluginPage.Dispose();
    }
}