using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Laminar_Core.PluginManagement
{
    static class InbuiltPluginFinder
    {
        private const string RelativePluginsPath = @"src\Plugins";

        public static IEnumerable<string> GetInbuiltPlugins()
        {
            string InbuiltPluginsFolder = Path.Combine(GetSolutionFileFolder().FullName, RelativePluginsPath);
            foreach (string projectPath in Directory.EnumerateDirectories(InbuiltPluginsFolder))
            {
                foreach (string dllPath in Directory.EnumerateFiles(GetOutputFromProjectFolder(projectPath), "*.dll"))
                {
                    yield return dllPath;
                }
            }
        }

        private static string GetOutputFromProjectFolder(string projectFolder)
        {
            string debugs = Path.Combine(projectFolder, @"bin\Debug");
            foreach (string path in Directory.EnumerateDirectories(debugs))
            {
                return path;
            }

            return null;
        }

        private static DirectoryInfo GetSolutionFileFolder()
        {
            DirectoryInfo directory = new(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }

            return directory;
        }
    }
}
