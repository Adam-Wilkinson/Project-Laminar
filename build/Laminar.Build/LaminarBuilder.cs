namespace Laminar.Build;

public static class LaminarBuilder
{
    private const string PluginFrameworkSourceGenerators = "src/PluginFramework/Laminar.PluginFramework.SourceGeneration/Laminar.PluginFramework.SourceGeneration.csproj";
    private const string PluginFramework = "src/PluginFramework/Laminar.PluginFramework/Laminar.PluginFramework.csproj";
    private const string PluginFrameworkVersion = "src/PluginFramework/Laminar.PluginFramework.Version/Laminar.PluginFramework.Version.csproj";
    private static readonly string[] Plugins =
    [
        "src/Plugins/BasicFunctionality/BasicFunctionality.csproj",
        "src/Plugins/BasicFunctionality.Avalonia/BasicFunctionality.Avalonia.csproj",
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

        if (currentPluginVersionValid && !File.Exists(
                Path.Combine(Dotnet.RepoRoot, ".nuget.local", $"Laminar.PluginFramework.{pluginVersion}.nupkg")))
        {
            Console.WriteLine("Unable to find correct plugin framework version");
            currentPluginVersionValid = false;
        }

        if (currentPluginVersionValid && !File.Exists(
                Path.Combine(Dotnet.RepoRoot, ".nuget.local", $"Laminar.PluginFramework.SourceGeneration.{pluginVersion}.nupkg")))
        {
            Console.WriteLine("Unable to find correct plugin framework source generation version");
            currentPluginVersionValid = false;
        }

        if (!currentPluginVersionValid)
        {
            Console.WriteLine("Rebuilding and restoring...");

            // Build plugin framework first
            await Dotnet.Pack(PluginFrameworkSourceGenerators, 
                Dotnet.PluginFrameworkVersion(pluginVersion), Dotnet.DoNotUseSharedCompilation);

            await Dotnet.Pack(PluginFramework, 
                Dotnet.PluginFrameworkVersion(pluginVersion), Dotnet.DoNotUseSharedCompilation);

            await SetPersistentPluginFrameworkVersion(pluginVersion);
            
            // Repo should be stable, restore to check:
            await Dotnet.Restore();
            await Dotnet.ShutdownBuildServer();
        }

        // Build plugins
        foreach (var plugin in Plugins)
        {
            await Dotnet.Build(plugin, Dotnet.NoRestore);
        }

        // Build app
        await Dotnet.Build(App, Dotnet.NoRestore);
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