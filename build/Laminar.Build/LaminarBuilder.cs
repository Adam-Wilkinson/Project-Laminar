using System.Text;
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
        "src/Application/Laminar.Avalonia/bin/Release/net10.0",
        "build/Laminar.Run/bin/Debug/net10.0",
        "build/Laminar.Run/bin/Release/net10.0"
    ];
    
    private const string App = "src/Application/Laminar.Avalonia/Laminar.Avalonia.csproj";
    private static readonly IDotnet Dotnet = new Dotnet();
    
    public static async Task Build()
    {
        await PluginFrameworkHandler.Setup();

        var lampackerLocal = Path.Combine(Dotnet.GetRepoRoot(), ".lampacker.local");
        
        // Build plugins
        foreach (var plugin in Plugins)
        {
            await Dotnet
                .Run("lampacker", "pack", $"\"src/Plugins/{plugin}/{plugin}.csproj\"", $"-o \"{lampackerLocal}\"")
                .ThrowOnError();
        }

        // Establish repositories
        foreach (var path in CopyDevRepositoryTo)
        {
            var absolutePath = Path.Combine(Dotnet.GetRepoRoot(), path);
            
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }

            var relativeLaminarPath = Path.GetRelativePath(absolutePath, lampackerLocal).Replace('\\', '/');
            var devPluginRepositories = Encoding.UTF8.GetBytes(
                $$"""
                  {
                      "repositories": [
                          {
                              "id": "dev",
                              "provider": "filesystem",
                              "path": "{{relativeLaminarPath}}"
                          }
                      ]
                  }
                  """);
                
            await using var fs = File.Create(Path.Combine(absolutePath, "repositories.json"));
            await fs.WriteAsync(devPluginRepositories);
        }
        
        // Build app
        await Dotnet.Build(App, IDotnet.NoRestore);
    }
}