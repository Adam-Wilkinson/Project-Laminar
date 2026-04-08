using System;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.UserData.FileNavigation;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.ViewModels.Design;

public static class DesignViewModel
{
    private static readonly IServiceProvider DesignServiceProvider = new ServiceCollection()
        .AddTransient<ILaminarFileBrowser, DesignFileBrowser>()
        .AddTransient<IDialogService, DialogService>()
        .AddViewModels()
        .BuildServiceProvider();
    
    private static readonly ILaminarFileBrowser DesignFileBrowser = new DesignFileBrowser();
    
    public static readonly FileNavigatorViewModel FileNavigator = DesignServiceProvider.GetRequiredService<FileNavigatorViewModel>();
    
    public static readonly MainControlViewModel MainControl = DesignServiceProvider.GetRequiredService<MainControlViewModel>();
    
    public static readonly TitleBarViewModel TitleBar = new();

    public static readonly SettingsViewModel Settings = new();
    
    public static readonly MainWindowViewModel MainWindow = new(MainControl, TitleBar, Settings);
}