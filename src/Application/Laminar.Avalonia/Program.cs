using Avalonia;

namespace Laminar.Avalonia;

public static class AppBootstrapper
{
    [STAThread]
    public static void Run(string[] args) => Program.BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
}

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => AppBootstrapper.Run(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
