using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Laminar.Avalonia.ToolSystem;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Implementation.UserData.FileNavigation;

namespace Laminar.Avalonia.ViewModels.Design;

public static class DesignViewModel
{
    private static readonly ILaminarFileBrowser MockFileBrowser = new MockFileBrowser();
    
    public static readonly FileNavigatorViewModel FileNavigator = new(MockFileBrowser, new DialogService());
    
    public static readonly MainControlViewModel MainControl = new(FileNavigator);
    
    public static readonly TitleBarViewModel TitleBar = new();

    public static readonly SettingsViewModel Settings = new();
    
    public static readonly MainWindowViewModel MainWindow = new(MainControl, TitleBar, Settings);
}