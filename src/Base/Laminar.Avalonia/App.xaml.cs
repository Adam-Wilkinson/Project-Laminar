using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Laminar.Avalonia.Views;
using System.Threading;

namespace Laminar.Avalonia;

public class App : Application
{
    public static DragDropHandler DragDropHandler { get; private set; }

    public static Laminar.Core.Instance LaminarInstance { get; } = new(SynchronizationContext.Current);

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
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
