using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Laminar.Implementation.Base.PluginLoading;

static class InbuiltPluginFinder
{
    private const string RelativePluginsPath = @"src\Plugins";

    public static IEnumerable<string> GetInbuiltPlugins(string path)
    {
        DirectoryInfo? solutionFileFolder = GetSolutionFileFolder(path);

        if (solutionFileFolder is not null)
        {
            string InbuiltPluginsFolder = Path.Combine(solutionFileFolder.FullName, RelativePluginsPath);
            foreach (string projectPath in Directory.EnumerateDirectories(InbuiltPluginsFolder))
            {
                string? outputFromProjectFolder = GetOutputFromProjectFolder(projectPath);

                if (outputFromProjectFolder is null)
                {
                    continue;
                }

                foreach (string dllPath in Directory.EnumerateFiles(outputFromProjectFolder, "*.dll"))
                {
                    yield return dllPath;
                }
            }
        }
    }

    private static string? GetOutputFromProjectFolder(string projectFolder)
    {
        string debugs = Path.Combine(projectFolder, @"bin\Debug");

        if (!Directory.Exists(debugs))
        {
            return null;
        }

        foreach (string path in Directory.EnumerateDirectories(debugs))
        {
            return path;
        }

        return null;
    }

    private static DirectoryInfo? GetSolutionFileFolder(string rootPath)
    {
        DirectoryInfo? directory = new FileInfo(rootPath).Directory;
        while (directory is not null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        return directory;
    }
}
