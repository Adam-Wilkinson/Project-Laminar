using System.Diagnostics;
using System.Text;
using Corvus.Text.Json;
using DotnetHelper;
using JsonWriterOptions = System.Text.Json.JsonWriterOptions;

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
        
        var tempDir = Path.Combine(parentDir.FullName, "obj", "Lampacker");
        await dotnet.Publish(csProjFile.FullName, ct, IDotnet.OutputDirectory(tempDir));

        var pluginDir = Path.Combine(parentDir.FullName, "plugin.json");

        if (!File.Exists(pluginDir))
        {
            throw new InvalidOperationException($"Could not find plugin.json file '{pluginDir}'");
        }
        
        await using var pluginDataFile = File.OpenRead(pluginDir);
        var pluginData = PluginData.Parse(pluginDataFile);

        var manifest = ManifestData.FromPluginData(pluginData);
        
        await using var manifestFile = File.Create(Path.Combine(tempDir, "manifest.json"));
        await using var jsonWriter = new System.Text.Json.Utf8JsonWriter(manifestFile, new JsonWriterOptions { Indented = true });
        manifest.WriteTo(jsonWriter);
        
        Debug.WriteLine($"Published plugin {(string)manifest.Version}");
    }
}