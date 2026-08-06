using System.Diagnostics;
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
        await dotnet.Publish(csProjFile.FullName);

        if (csProjFile.Directory is not { } parentDir)
        {
            throw new InvalidOperationException($"Could not find parent directory for .csproj file '{csProjFile}'");
        }

        var pluginDir = Path.Combine(parentDir.FullName, "plugin.json");

        if (!File.Exists(pluginDir))
        {
            throw new InvalidOperationException($"Could not find plugin.json file '{pluginDir}'");
        }
        
        await using var pluginDataFile = File.OpenRead(pluginDir);
        var pluginData = PluginData.Parse(pluginDataFile);
        var pluginVersion = GetPluginVersion(pluginData);
        
        Debug.WriteLine($"Published plugin {pluginVersion}");
    }

    public static string GetPluginVersion(PluginData pluginData)
    {
        var pluginFullName = $"{(string)pluginData.Id}.{(int)pluginData.MajorVersion}.{(int)pluginData.MinorVersion}";
        
        if (pluginData.PatchVersion.IsJsonNumber)
        {
            pluginFullName += $".{(int)pluginData.PatchVersion}";
        }

        if (pluginData.PrereleaseVersion.AsString.HasJsonElementBacking)
        {
            pluginFullName += "-" + (string)pluginData.PrereleaseVersion;
        }

        return pluginFullName;
    }
}