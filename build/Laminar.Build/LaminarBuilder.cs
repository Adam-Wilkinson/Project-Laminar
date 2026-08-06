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

    private static readonly string[] CopyDevRepositoryTo =
    [
        "src/Application/Laminar.Avalonia/bin/Debug/net10.0",
        "src/Application/Laminar.Avalonia/bin/Release/net10.0"
    ];
    
    private static readonly byte[] DevPluginRepository =
    [
        .. """
           {
               "repositories": [
                   {
                       "id": "dev",
                       "path": "../../../../../.lampacker.local"
                   }
               ]
           }
           """u8
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
        
        // Establish repositories
        foreach (var path in CopyDevRepositoryTo)
        {
            var absolutePath = Path.Combine(Dotnet.GetRepoRoot(), path);
            
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }

            await using var fs = File.Create(Path.Combine(absolutePath, "repositories.json"));
            await fs.WriteAsync(DevPluginRepository);
        }
        
        // Build app
        await Dotnet.Build(App, IDotnet.NoRestore);
        await Dotnet.ShutdownBuildServer();
    }
}