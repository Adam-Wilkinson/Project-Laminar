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
    private static readonly IDotnet Dotnet = new Dotnet();
    
    public static async Task Build()
    {
        await PluginFrameworkHandler.Setup();
        
        // Build plugins
        foreach (var plugin in Plugins)
        {
            await Dotnet
                .Run("lampacker", "pack", $"src/Plugins/{plugin}/{plugin}.csproj", "-o .lampacker.local")
                .ThrowOnError();
            await Dotnet.ShutdownBuildServer();
        }

        // Build app
        await Dotnet.Build(App, IDotnet.NoRestore);
        await Dotnet.ShutdownBuildServer();
    }
}