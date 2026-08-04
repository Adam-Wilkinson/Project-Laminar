using System.Diagnostics;
using DotnetHelper;

namespace Laminar.PluginFramework.CLI.Packing;

public static class PackageBuilder
{
    public static async Task BuildAsync(FileInfo csProjFile, DirectoryInfo? outputDirectory, CancellationToken ct)
    {
        await Dotnet.Publish(csProjFile.FullName);

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

        var pluginFullName = $"{(string)pluginData.Id}.{(int)pluginData.MajorVersion}.{(int)pluginData.MinorVersion}.{(int)pluginData.PatchVersion}";
        var prerelease = (string)pluginData.PrereleaseVersion;
        if (!string.IsNullOrEmpty(prerelease))
        {
            pluginFullName += "-" + prerelease;
        }
        
        Debug.WriteLine($"Published plugin {pluginFullName}");
    }
}