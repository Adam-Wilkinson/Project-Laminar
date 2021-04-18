namespace Laminar_Avalonia
{
    using Avalonia;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Markup.Xaml;
    using Avalonia.Threading;
    using Laminar_Avalonia.Views;
    using System.Threading;

    public class App : Application
    {
        public static DragDropHandler DragDropHandler { get; private set; }

        public static Laminar_Core.Instance LaminarInstance { get; } = new Laminar_Core.Instance(SynchronizationContext.Current);

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
}
