using System.Diagnostics;

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
    
    private static async Task<DotnetResult> RunDotnet(string repoRoot, string command, string args)
    {
        Console.WriteLine($"> dotnet {command} {args}");

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
                ["DOTNET_CLI_HOME"] = Path.Combine(repoRoot, ".dotnet-runner-cache"),
                ["MSBUILDDISABLENODEREUSE"] = "1",
                ["DOTNET_NOLOGO"] = "1"
            }
        };

        using var process = Process.Start(psi)!;

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var result = new DotnetResult(process.ExitCode, await stdoutTask, await stderrTask, command);

        return result;
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