using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace DotnetHelper;

public partial class Dotnet : IDotnet
{
    private static readonly Regex PidRegex = GeneratePigRegex();
    
    public Dotnet(string buildConfig = "")
    {
#if DEBUG
        BuildConfig = IDotnet.Debug;
#endif

        if (!string.IsNullOrWhiteSpace(buildConfig))
        {
            BuildConfig = buildConfig;
        }
        
        DotNetPath = FindDotnet();
    }
    
    public string DotNetPath { get; }

    public string GetRepoRoot(string? path = null)
    {
        return TryGetRepoRoot(path) ?? throw new InvalidOperationException($"Unable to locate repo root from path '{path}'");
    }
    
    public string? TryGetRepoRoot(string? path = null)
    {
        string? fullPath = Path.GetFullPath(path ?? AppContext.BaseDirectory);
        
        while (fullPath is not null)
        {
            if (File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory) && Directory.EnumerateFiles(fullPath).Any(x => x.EndsWith(".sln") || x.EndsWith(".slnx")))
            {
                return fullPath;
            }

            fullPath = Path.GetDirectoryName(fullPath);
        }

        return null;
    }
    
    public Task<DotnetResult> Build(string path, params string[] args) 
        => RunDotnet(path, "build", $"{path} -c {BuildConfig} {string.Join(" ", args)}")
            .ThrowOnError();

    public Task<DotnetResult> Pack(string path, params string[] args)
        => RunDotnet(path, "pack", $"{path} -c {BuildConfig} {string.Join(" ", args)}")
            .ThrowOnError();

    public Task<DotnetResult> Publish(string path, CancellationToken ct, params string[] args)
        => RunDotnet(path, "publish", $"{path} -c {BuildConfig} {string.Join(" ", args)}", ct)
            .ThrowOnError(); 
    
    public Task<DotnetResult> Restore(string? path = null, params string[] args)
        => RunDotnet(path, "restore",  $"{path} {string.Join(" ", args)}");

    public Task<DotnetResult> ShutdownBuildServer(params string[] args)
        => RunDotnet(null, "build-server", "shutdown");
    
    public Task<DotnetResult> New(string template)
        => RunDotnet(null, "new", template);

    public Task<DotnetResult> Tool(string command, params string[] args)
        => RunDotnet(null, "tool", $"{command} {string.Join(" ", args)}");
    
    public string BuildConfig { get; private set; } = IDotnet.Release;
    

    private async Task<DotnetResult> RunDotnet(
        string? path,
        string command,
        string args,
        CancellationToken ct = default)
    {
        Console.WriteLine($"> dotnet {command} {args}");

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        var repoRoot = TryGetRepoRoot() ?? TryGetRepoRoot(path) ?? throw new InvalidOperationException("Unable to find repo root");
        
        var psi = new ProcessStartInfo
        {
            FileName = DotNetPath,
            Arguments = $"{command} {args}",
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            Environment =
            {
                ["DOTNET_CLI_HOME"] = Path.Combine(repoRoot, ".dotnet-runner-cache"),
                ["MSBUILDDISABLENODEREUSE"] = "1",
                ["DOTNET_NOLOGO"] = "1",
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

        await process.WaitForExitAsync(ct);

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
    
    private static string FindDotnet()
    {
        // First choice: let the OS resolve it through PATH
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            process!.WaitForExit();

            if (process.ExitCode == 0)
                return "dotnet";
        }
        catch (Win32Exception)
        {
            // Not on PATH
        }

        // Fallback One: Get environment variable
        if (Environment.GetEnvironmentVariable("DOTNET_ROOT") is { } root)
        {
            return Path.Combine(root, "dotnet");
        }
        
        // Fallback Two: Some hardcoded options
        var candidates = new List<string>();

        if (OperatingSystem.IsWindows())
        {
            candidates.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "dotnet",
                "dotnet.exe"));
        }
        else
        {
            candidates.Add("/usr/bin/dotnet");
            candidates.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".dotnet",
                "dotnet"));
        }
        
        foreach (var candidate in candidates.Where(File.Exists))
        {
            return candidate;
        }

        throw new InvalidOperationException("Could not find dotnet. Please install the .NET SDK and ensure it is available on PATH.");
    }

    [GeneratedRegex(@"\((\d+)\)", RegexOptions.Compiled)]
    private static partial Regex GeneratePigRegex();
}