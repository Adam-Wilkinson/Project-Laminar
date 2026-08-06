namespace DotnetHelper;

public interface IDotnet
{
    public const string Debug = "Debug";

    public const string Release = "Release";

    public static string PluginFrameworkVersion(string frameworkVersion) => $"/p:PluginFrameworkVersion={frameworkVersion}";

    public static string OutputDirectory(string outputDirectory) => $"--output \"{outputDirectory}\"";
    
    public const string DoNotUseSharedCompilation = "/p:UseSharedCompilation=false";

    public const string EmitPluginFrameworkVersion = "/p:EmitPluginFrameworkVersion=true";

    public const string NoRestore = "--no-restore";

    public const string Prerelease = "--prerelease";

    public const string Local = "--local";

    public Task<DotnetResult> Build(string path, params string[] args);

    public Task<DotnetResult> Pack(string path, params string[] args);

    public Task<DotnetResult> Publish(string path, CancellationToken ct, params string[] args);
    
    public Task<DotnetResult> Restore(string? path = null, params string[] args);

    public Task<DotnetResult> ShutdownBuildServer(params string[] args);

    public Task<DotnetResult> New(string template);

    public Task<DotnetResult> Tool(string command, params string[] args);
    
    public string GetRepoRoot(string? path = null);

    public string? TryGetRepoRoot(string? path = null);
}

public static class DotnetExtensions
{
    extension(IDotnet dotnet)
    {
        public Task<DotnetResult> Publish(string path, params string[] args) 
            => dotnet.Publish(path, CancellationToken.None, args);
    }
}