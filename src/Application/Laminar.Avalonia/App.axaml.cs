using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Avalonia.ViewModels;
using Laminar.Avalonia.Views;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Implementation.Extensions;
using Laminar.Implementation.Extensions.ServiceInitializers;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia;
public partial class App : Application
{
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
                .AddLaminarServices()
                .AddDescendantsTransient<ViewModelBase>()
                .AddDescendantsSingleton<IBeforeApplicationBuiltTarget>()
                .AddDescendantsSingleton<IAfterApplicationBuiltTarget>()
                .AddSingleton(desktop.MainWindow.StorageProvider)
                .AddSingleton<TopLevel>(desktop.MainWindow)
                .BuildServiceProvider();

            services.InitializeLaminar(FrontendDependency.Avalonia);
            DataTemplates.Add(new DataInterfaceTemplate(services.GetRequiredService<IDataInterfaceFactory>()));
            
            services.GetServices<IBeforeApplicationBuiltTarget>().Initialize();
            
            desktop.MainWindow.DataContext = services.GetRequiredService<MainWindowViewModel>();

            services.GetServices<IAfterApplicationBuiltTarget>().Initialize();
        }

        base.OnFrameworkInitializationCompleted();
    }
}