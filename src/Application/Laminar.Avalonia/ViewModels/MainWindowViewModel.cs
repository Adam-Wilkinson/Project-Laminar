using System.Windows.Input;
using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.ViewModels.Services;

namespace Laminar.Avalonia.ViewModels;
public partial class MainWindowViewModel : ViewModelBase, IUndoTarget
{
    [ObservableProperty] private bool _settingsOpen;
    [ObservableProperty] private bool _sidebarExpanded;
    
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

    partial void OnSidebarExpandedChanged(bool value)
    {
        MainControl.SidebarExpanded = value;
    }

    [RelayCommand]
    public void Undo() 
    {
        
    }

    ICommand IUndoTarget.UndoCommand => UndoCommand;
}