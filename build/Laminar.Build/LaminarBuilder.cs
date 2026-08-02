using DotnetHelper;
using Laminar.PluginFrameworkSetup;

namespace Laminar.Build;

public static class LaminarBuilder
{
    private static readonly string[] Plugins =
    [
        "BasicFunctionality",
        "BasicFunctionality.Avalonia",
    ];
    private const string App = "src/Application/Laminar.Avalonia/Laminar.Avalonia.csproj";
    
    public static async Task Build()
    {
        await PluginFrameworkHandler.Setup();
        
        // Build plugins
        foreach (var plugin in Plugins)
        {
            await Dotnet.Build($"src/Plugins/{plugin}/{plugin}.csproj", Dotnet.NoRestore);
            await Dotnet.ShutdownBuildServer();
        }

        // Build app
        await Dotnet.Build(App, Dotnet.NoRestore);
        await Dotnet.ShutdownBuildServer();
    }
}