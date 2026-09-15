using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(
        MainControlViewModel mainControl, 
        SettingsViewModel settings)
    {
        MainControl = mainControl;
        Settings = settings;
        SidebarExpanded = MainControl.SidebarExpanded;
        MainControl.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainControl.SidebarExpanded))
                SidebarExpanded = MainControl.SidebarExpanded;
        };
    }
    public SettingsViewModel Settings { get; }

    public MainControlViewModel MainControl { get; }
    
    [ObservableProperty]
    public partial bool SettingsOpen { get; set; }

    [ObservableProperty]
    public partial bool SidebarExpanded { get; set; }

    partial void OnSidebarExpandedChanged(bool value)
    {
        MainControl.SidebarExpanded = value;
    }
}