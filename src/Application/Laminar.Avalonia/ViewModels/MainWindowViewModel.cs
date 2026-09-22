using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Primitives;

namespace Laminar.Avalonia.ViewModels;

public partial class MainWindowViewModel(MainControlViewModel mainControl, SettingsViewModel settings) 
    : ViewModelBase(mainControl)
{
    public SettingsViewModel Settings { get; } = settings;

    public MainControlViewModel MainControl { get; } = mainControl;
    
    [ObservableProperty]
    public partial bool SettingsOpen { get; set; }

    [ObservableProperty] 
    public partial bool SidebarExpanded { get; set; } = mainControl.SidebarExpanded;

    protected override void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainControl.SidebarExpanded)) 
            SidebarExpanded = MainControl.SidebarExpanded;
    }

    partial void OnSidebarExpandedChanged(bool value)
    {
        MainControl.SidebarExpanded = value;
    }
}