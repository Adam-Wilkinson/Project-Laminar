using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Avalonia.ViewModels;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Avalonia.Views;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Implementation.Extensions;
using Laminar.Implementation.Extensions.ServiceInitializers;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Laminar.Avalonia;
public partial class App : Application
{
    private static readonly string LocalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Project Laminar");
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
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
                        .WriteTo.File(Path.Combine(LocalFolder, "logs.txt"))
                        .CreateLogger()))
                .AddSingleton<IDialogFactory>(new DialogFactory().AddMessageBox())
                .AddSingleton<IDialogManager, DialogManager>()
                .AddSingleton<IDialogService, DialogService>()
                .BuildServiceProvider();
            
            services.InitializeLaminar<App>();
            services.GetServices<IBeforeApplicationBuiltTarget>().Initialize();
            
            desktop.MainWindow.DataContext = services.GetRequiredService<MainWindowViewModel>();

            services.GetServices<IAfterApplicationBuiltTarget>().Initialize();
            
            IPluginLoader pluginLoader = services.GetRequiredService<IPluginLoader>();
            foreach (IPlugin avaloniaPlugin in services.GetServices<IPlugin>())
            {
                pluginLoader.Register(avaloniaPlugin);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}