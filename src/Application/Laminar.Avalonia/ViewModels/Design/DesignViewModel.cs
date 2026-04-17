using System;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Storage.FileExplorer;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.ViewModels.Design;

public static class DesignViewModel
{
    private static readonly IServiceProvider DesignServiceProvider = new ServiceCollection()
        .AddTransient<ILaminarFileBrowser, DesignFileBrowser>()
        .AddTransient<IDialogService, global::HanumanInstitute.MvvmDialogs.Avalonia.DialogService>()
        .AddViewModels()
        .BuildServiceProvider();
    
    public static readonly FileNavigatorViewModel FileNavigator = DesignServiceProvider.GetRequiredService<FileNavigatorViewModel>();
    
    public static readonly MainControlViewModel MainControl = DesignServiceProvider.GetRequiredService<MainControlViewModel>();

    public static readonly SettingsViewModel Settings = DesignServiceProvider.GetRequiredService<SettingsViewModel>();
    
    public static readonly MainWindowViewModel MainWindow = DesignServiceProvider.GetRequiredService<MainWindowViewModel>();

    public static readonly LaminarDialogViewModel DialogTest = new()
    {
        Title = "A dialog box",
        Message = "This is a message to describe what the dialog does",
        Options = [new DialogOption("No"), new DialogOption("Yes"), new DialogOption("This is longer")],
        ShowRememberAnswer = true,
    };
}