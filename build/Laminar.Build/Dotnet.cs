using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Laminar.Build;

public static class Dotnet
{
    public const string Debug = "Debug";
    public const string Release = "Release";
    
    public static readonly string RepoRoot;
    
    static Dotnet()
    {
#if DEBUG
        BuildConfig = Debug;
#endif
        
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                RepoRoot = dir.FullName;
                break;   
            }

            dir = dir.Parent;
        }
        
        if (RepoRoot is null) throw new InvalidOperationException("Could not find repo root");
    }
    
    public static Task<DotnetResult> Build(string path, params string[] args) 
        => RunDotnet(RepoRoot, "build", $"{path} -c {BuildConfig} {string.Join(" ", args)}")
            .ThrowOnError();

    public static Task<DotnetResult> Pack(string path, params string[] args)
        => RunDotnet(RepoRoot, "pack", $"{path} -c {BuildConfig} {string.Join(" ", args)}")
            .ThrowOnError();

    public static Task<DotnetResult> Restore(params string[] args)
        => RunDotnet(RepoRoot, "restore",  $"ProjectLaminar.slnx {string.Join(" ", args)}");

    public static Task<DotnetResult> ShutdownBuildServer(params string[] args)
        => RunDotnet(RepoRoot, "build-server", "shutdown");
    
    public static string BuildConfig { get; private set; } = Release;
    
    public static string PluginFrameworkVersion(string frameworkVersion) => $"/p:PluginFrameworkVersion={frameworkVersion}";

    public const string DoNotUseSharedCompilation = "/p:UseSharedCompilation=false";
    
    public const string EmitPluginFrameworkVersion = "/p:EmitPluginFrameworkVersion=true";

    public const string NoRestore = "--no-restore";
    
    private static readonly Regex PidRegex = new(@"\((\d+)\)", RegexOptions.Compiled);

    private static async Task<DotnetResult> RunDotnet(
        string repoRoot,
        string command,
        string args)
    {
        Console.WriteLine($"> dotnet {command} {args}");

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"{command} {args}",
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            Environment =
            {
                ["DOTNET_CLI_HOME"] =
                    Path.Combine(repoRoot, ".dotnet-runner-cache"),

                ["MSBUILDDISABLENODEREUSE"] = "1",
                ["DOTNET_NOLOGO"] = "1",
                ["DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER"] = "1",
            }
        };

        using var process = new Process();
        process.StartInfo = psi;
        process.EnableRaisingEvents = true;

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data == null)
                return;
            
            lock (stdout)
                stdout.AppendLine(e.Data);

            HandlePotentialLock(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data == null)
                return;
            
            lock (stderr)
                stderr.AppendLine(e.Data);

            HandlePotentialLock(e.Data);
        };

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return new DotnetResult(
            process.ExitCode,
            stdout.ToString(),
            stderr.ToString(),
            command);
    }
    
    private static readonly HashSet<int> KilledPids = [];

    private static void HandlePotentialLock(string line)
    {
        if (!line.Contains("because it is being used by another process"))
            return;

        foreach (var pid in ExtractPids(line))
        {
            lock (KilledPids)
            {
                if (!KilledPids.Add(pid))
                    return;
            }

            try
            {
                var proc = Process.GetProcessById(pid);
                if (!proc.ProcessName.Contains("dotnet"))
                    return;
                
                Console.WriteLine($"Killing locking process '{proc.ProcessName}' ({pid})");
                proc.Kill(entireProcessTree: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to kill process '{pid}': {ex.Message}");
            }   
        }
    }
    
    private static List<int> ExtractPids(string line)
    {
        var pids = new List<int>();

        const string marker = "The file is locked by:";

        var markerIndex = line.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
            return pids;

        var firstQuote = line.IndexOf('"', markerIndex);
        if (firstQuote < 0)
            return pids;

        var secondQuote = line.IndexOf('"', firstQuote + 1);
        if (secondQuote < 0)
            return pids;

        var insideQuotes = line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);

        foreach (Match match in PidRegex.Matches(insideQuotes))
        {
            if (int.TryParse(match.Groups[1].Value, out var pid))
            {
                pids.Add(pid);
            }
        }

        return pids;
    }
}

public record DotnetResult(int ExitCode, string StdOut, string StdErr, string Command);

internal static class DotnetResultHelpers
{
    extension(Task<DotnetResult> resultTask)
    {
        public async Task<DotnetResult> ThrowOnError()
        {
            var result = await resultTask;

            if (result.ExitCode != 0)
            {
                Console.WriteLine($"dotnet {result.Command} failed. Exit code: {result.ExitCode}");
                Console.WriteLine("StdOut:");
                Console.WriteLine(result.StdOut);
                Console.WriteLine("StdErr:");
                await Console.Error.WriteLineAsync(result.StdErr);
                throw new Exception($"dotnet {result.Command} failed. Exit code: {result.ExitCode}");
            }

            return result;
        }
    }
}