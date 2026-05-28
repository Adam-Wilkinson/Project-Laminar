namespace Laminar.Build;

public static class LaminarBuilder
{
    private const string PluginFrameworkVersion = "src/PluginFramework/Laminar.PluginFramework.Version/Laminar.PluginFramework.Version.csproj";
    private static readonly string[] PluginFrameworkPackages =
    [
        "Laminar.PluginFramework.Core",
        "Laminar.PluginFramework.SourceGeneration",
        "Laminar.PluginFramework",
    ];
    private static readonly string[] Plugins =
    [
        "BasicFunctionality",
        "BasicFunctionality.Avalonia",
    ];
    private const string App = "src/Application/Laminar.Avalonia/Laminar.Avalonia.csproj";
    private static readonly string PluginFrameworkVersionFile = Path.Combine(Dotnet.RepoRoot, ".nuget.local", "PluginFramework.Version.props");
    
    public static async Task Build()
    {
        await Dotnet.ShutdownBuildServer();

        // Get PluginFramework version via probe project
        var output =
            await Dotnet.Pack(PluginFrameworkVersion, Dotnet.EmitPluginFrameworkVersion, Dotnet.DoNotUseSharedCompilation);

        var pluginVersion = ExtractPluginFrameworkVersion(output.StdOut);
        var persistentPluginVersion = await GetPersistentPluginFrameworkVersion();
        var currentPluginVersionValid = true;

        if (!pluginVersion.Equals(persistentPluginVersion))
        {
            Console.WriteLine(
                $"PluginFramework version changed: {persistentPluginVersion} -> {pluginVersion}");
            currentPluginVersionValid = false;
        }

        foreach (var pluginFramework in PluginFrameworkPackages)
        {
            if (!currentPluginVersionValid) break;
            if (!File.Exists(Path.Combine(Dotnet.RepoRoot, ".nuget.local", $"{pluginFramework}.{pluginVersion}.nupkg")))
            {
                Console.WriteLine("Unable to find package {0} of expected version {1}", pluginFramework, pluginVersion);
                currentPluginVersionValid = false;
            }
        }

        if (!currentPluginVersionValid)
        {
            Console.WriteLine("Rebuilding and restoring...");

            foreach (var pluginFramework in PluginFrameworkPackages)
            {
                await Dotnet.Pack($"src/PluginFramework/{pluginFramework}/{pluginFramework}.csproj",
                    Dotnet.PluginFrameworkVersion(pluginVersion), Dotnet.DoNotUseSharedCompilation);
            }

            await SetPersistentPluginFrameworkVersion(pluginVersion);
            
            // Repo should be stable, restore to check:
            await Dotnet.Restore();
            await Dotnet.ShutdownBuildServer();
        }

        // Build plugins
        foreach (var plugin in Plugins)
        {
            await Dotnet.Build($"src/Plugins/{plugin}/{plugin}.csproj", Dotnet.NoRestore, Dotnet.DoNotUseSharedCompilation);
            await Dotnet.ShutdownBuildServer();
        }

        // Build app
        await Dotnet.Build(App, Dotnet.NoRestore, Dotnet.DoNotUseSharedCompilation);
        await Dotnet.ShutdownBuildServer();
    }
    
    private static string ExtractPluginFrameworkVersion(string buildOutput)
    {
        const string prefix = "PLUGINFRAMEWORK_VERSION=";

        var line = buildOutput
            .Split('\n')
            .Select(l => l.Trim())
            .FirstOrDefault(l => l.Contains(prefix));

        if (line is null)
            throw new Exception("PluginFramework version not emitted");

        return line[(line.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length)..].Trim();
    }

    private static async Task SetPersistentPluginFrameworkVersion(string version)
    {
        var tempFile = PluginFrameworkVersionFile + ".tmp";
        var contents =
            $"""
             <Project>
                 <PropertyGroup Condition="'$(PluginFrameworkVersion)' == ''">
                     <PluginFrameworkVersion>{version}</PluginFrameworkVersion>
                 </PropertyGroup>
             </Project>                               
             """;

        await File.WriteAllTextAsync(tempFile, contents);
        File.Move(tempFile, PluginFrameworkVersionFile, overwrite: true);
    }

    private static async Task<string?> GetPersistentPluginFrameworkVersion()
    {
        if (!File.Exists(PluginFrameworkVersionFile))
        {
            return null;
        }

        var contents = await File.ReadAllTextAsync(PluginFrameworkVersionFile);
        int openingTagPosition = contents.IndexOf("<PluginFrameworkVersion>", StringComparison.Ordinal) + "<PluginFrameworkVersion>".Length;
        int closingTagPosition = contents.IndexOf("</PluginFrameworkVersion", StringComparison.Ordinal);
        return contents.Substring(openingTagPosition, closingTagPosition - openingTagPosition).Trim();
    }
}