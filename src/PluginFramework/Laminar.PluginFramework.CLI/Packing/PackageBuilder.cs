using DotnetHelper;

namespace Laminar.PluginFramework.CLI.Packing;

public static class PackageBuilder
{
    public static async Task BuildAsync(FileInfo csProjFile, DirectoryInfo? outputDirectory, CancellationToken ct)
    {
        await Dotnet.Publish(csProjFile.FullName);
    }
}