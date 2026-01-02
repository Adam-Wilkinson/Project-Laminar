using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Laminar.Avalonia.ToolSystem;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Implementation.UserData.FileNavigation;

namespace Laminar.Avalonia.ViewModels.Design;

public static class DesignViewModel
{
    private static readonly IUserActionManager MockUserActionManager = new MockUserActionManager();
    
    public static readonly FileNavigatorViewModel FileNavigator = new(MockUserActionManager, new MockDataManager(), new LaminarStorageItemFactory(null), new DialogService());
    
    public static readonly MainControlViewModel MainControl = new(FileNavigator);
    
    public static readonly TitleBarViewModel TitleBar = new();

    public static readonly SettingsViewModel Settings = new();
    
    public static readonly MainWindowViewModel MainWindow = new(MainControl, TitleBar, Settings);
}