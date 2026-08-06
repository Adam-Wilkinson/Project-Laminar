using System.IO.Compression;
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

        outputDirectory ??= parentDir.CreateSubdirectory("bin");
        
        var tempDir = Path.Combine(parentDir.FullName, "obj", "Lampacker");
        await dotnet.Publish(csProjFile.FullName, ct, IDotnet.OutputDirectory(tempDir));

        var pluginDir = Path.Combine(parentDir.FullName, "plugin.json");

        if (!File.Exists(pluginDir))
        {
            throw new InvalidOperationException($"Could not find plugin.json file '{pluginDir}'");
        }
        
        await using var pluginDataFile = File.OpenRead(pluginDir);
        var pluginData = PluginData.Parse(pluginDataFile);

        var manifest = ManifestData.FromPluginData(pluginData)
            .WithEntrypoint(Path.GetFileNameWithoutExtension(csProjFile.FullName) + ".dll");
        
        await using (var manifestFile = File.Create(Path.Combine(tempDir, "manifest.json")))
        await using (var jsonWriter =
                     new System.Text.Json.Utf8JsonWriter(manifestFile, new JsonWriterOptions { Indented = true }))
        {
            manifest.WriteTo(jsonWriter);
        }

        if (!Directory.Exists(outputDirectory.FullName))
        {
            Directory.CreateDirectory(outputDirectory.FullName);
        }
        
        var pluginFile = Path.Combine(outputDirectory.FullName, $"{(string)manifest.Id}.{(string)manifest.Version}.plpkg");

        if (File.Exists(pluginFile))
        {
            Console.WriteLine($"The plugin file {pluginFile} already exists. Version increment required to overwrite");
            return;
        }
        
        await ZipFile.CreateFromDirectoryAsync(
            sourceDirectoryName: tempDir,
            destinationArchiveFileName: Path.Combine(outputDirectory.FullName, $"{(string)manifest.Id}.{(string)manifest.Version}.plpkg"), 
            cancellationToken: ct);
    }
}