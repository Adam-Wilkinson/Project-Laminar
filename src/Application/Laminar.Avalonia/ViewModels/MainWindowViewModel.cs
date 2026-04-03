using System.ComponentModel;
using System.Windows.Input;
using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels;

public interface IRootWindowViewModel : INotifyPropertyChanged;

public partial class MainWindowViewModel : ViewModelBase, IRootWindowViewModel, IActionScope
{
    public MainWindowViewModel(MainControlViewModel mainControl, TitleBarViewModel titlebar, SettingsViewModel settings)
    {
        MainControl = mainControl;
        TitleBar = titlebar;
        Settings = settings;
        TitleBar.MainWindow = this;
        SidebarExpanded = MainControl.SidebarExpanded;
        MainControl.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainControl.SidebarExpanded))
                SidebarExpanded = MainControl.SidebarExpanded;
        };
    }
    
    public TitleBarViewModel TitleBar { get; }

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