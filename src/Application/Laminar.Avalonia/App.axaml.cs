using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Avalonia.UserActionHandlers;
using Laminar.Avalonia.ViewModels;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Avalonia.Views;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.DataManagement;
using Laminar.Implementation.Extensions;
using Laminar.Implementation.Extensions.ServiceInitializers;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Laminar.Avalonia;
public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            var services = new ServiceCollection()
                .AddLaminarServices(FrontendDependency.Avalonia)
                .AddViewModels()
                .AddDescendantsSingleton<IBeforeApplicationBuiltTarget>()
                .AddDescendantsSingleton<IAfterApplicationBuiltTarget>()
                .AddDescendantsSingleton<IPlugin>()
                .AddSingleton(desktop.MainWindow.StorageProvider)
                .AddSingleton<TopLevel>(desktop.MainWindow)
                .AddSingleton<Application>(this)
                .AddLogging(builder => builder.AddSerilog(
                    new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .WriteTo.Console()
                        .WriteTo.File(DataLocations.LocalDataFolder.ChildPath("logs.txt").ToString())
                        .CreateLogger()))
                .AddSingleton<IDialogFactory>(new DialogFactory())
                .AddSingleton<IDialogManager, DialogManager>()
                .AddSingleton<IDialogService, HanumanInstitute.MvvmDialogs.Avalonia.DialogService>()
                .AddUserActionHandlers()
                .BuildServiceProvider();

            services.InitializeLaminar<App>();
            services.GetServices<IBeforeApplicationBuiltTarget>().Initialize();
            desktop.MainWindow.DataContext = services.GetRequiredService<MainWindowViewModel>();
            services.GetServices<IAfterApplicationBuiltTarget>().Initialize();
            services.GetRequiredService<IPluginLoader>().Register(services.GetServices<IPlugin>());
        }

        base.OnFrameworkInitializationCompleted();
    }
}