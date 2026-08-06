using System.Diagnostics;
using System.Text;
using Corvus.Text.Json;
using DotnetHelper;

namespace Laminar.PluginFramework.CLI.Packing;

public static class PackageBuilder
{
    public static async Task BuildAsync(
        FileInfo csProjFile, 
        DirectoryInfo? outputDirectory,
        IDotnet dotnet,
        CancellationToken ct)
    {
        if (csProjFile.Directory is not { } parentDir)
        {
            throw new InvalidOperationException($"Could not find parent directory for .csproj file '{csProjFile}'");
        }
        
        outputDirectory ??= new DirectoryInfo(Path.Combine(csProjFile.Directory.FullName, "temp"));
        await dotnet.Publish(csProjFile.FullName, ct, IDotnet.OutputDirectory(outputDirectory.FullName));

        var pluginDir = Path.Combine(parentDir.FullName, "plugin.json");

        if (!File.Exists(pluginDir))
        {
            throw new InvalidOperationException($"Could not find plugin.json file '{pluginDir}'");
        }
        
        using var workspace = JsonWorkspace.Create();
        await using var pluginDataFile = File.OpenRead(pluginDir);
        var pluginData = PluginData.Parse(pluginDataFile);

        var manifest = ManifestData.FromPluginData(pluginData);
        
        await using var manifestWriter = File.Create(Path.Combine(outputDirectory.FullName, "manifest.json"));
        var bytes = Encoding.UTF8.GetBytes(manifest.ToString());
        await manifestWriter.WriteAsync(bytes, ct);
        
        Debug.WriteLine($"Published plugin {(string)manifest.Version}");
    }
}