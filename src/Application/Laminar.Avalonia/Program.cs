using System.Runtime.Loader;
using Avalonia;
using Bootstrapping;

namespace Laminar.Avalonia;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => WithLoadContext(null, args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() => BuildAvaloniaApp(null);

    [STAThread]
    public static void WithLoadContext(AssemblyLoadContext? loadContext, string[] args) =>
        BuildAvaloniaApp(loadContext).StartWithClassicDesktopLifetime(args);
    
    public static AppBuilder BuildAvaloniaApp(AssemblyLoadContext? defaultLoadContext)
        => AppBuilder.Configure(() => new App { DefaultLoadContext = defaultLoadContext })
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}

[ApplicationBootstrapper]
public class ApplicationBootstrapper : IApplicationBootstrapper
{
    [STAThread]
    public Task Run(AssemblyLoadContext defaultContext, string[] args)
    {
        try
        {
            Program.WithLoadContext(defaultContext, args);
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }
}