using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace Laminar.Implementation.Base.PluginLoading;

internal static class InbuiltPluginFinder
{
    private const string RelativePluginsPath = @"src\Plugins";

    public static IEnumerable<string> GetInbuiltPlugins(string path)
    {
        if (GetSolutionFileFolder(path) is not { } solutionFileFolder) yield break;

        var netTargetName = GetNetTargetFolder();
        
        var inbuiltPluginsFolder = Path.Combine(solutionFileFolder.FullName, RelativePluginsPath);
        foreach (var projectPath in Directory.EnumerateDirectories(inbuiltPluginsFolder))
        {            
            var correctNetVersionFolder = Path.Combine(projectPath, @"bin\Debug", netTargetName);

            if (!Directory.Exists(correctNetVersionFolder))
            {
                Debug.WriteLine($"Unable to find correct net version folder for project {projectPath}");
                continue;
            }
            
            foreach (var dllPath in Directory.EnumerateFiles(correctNetVersionFolder, "*.dll"))
            {
                yield return dllPath;
            }
        }
    }


    private static DirectoryInfo? GetSolutionFileFolder(string rootPath)
    {
        var directory = new FileInfo(rootPath).Directory;
        while (directory is not null && directory.GetFiles("*.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory;
    }

    private static string GetNetTargetFolder()
    {
        var directory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        while (directory is not null && directory.Name is not ("Debug" or "Release"))
        {
            if (directory.Parent?.Name is "Debug" or "Release")
            {
                return directory.Name;
            }
            
            directory = directory.Parent;
        }

        throw new Exception();
    }
}
