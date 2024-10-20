using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Laminar.Avalonia.ViewModels;
using Laminar.Avalonia.Views;

namespace Laminar.Avalonia;

public class App : Application
{
    public static Implementation.Instance LaminarInstance { get; } = new(SynchronizationContext.Current, PluginFramework.Registration.FrontendDependency.Avalonia);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
