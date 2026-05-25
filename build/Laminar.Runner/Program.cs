using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Bootstrapping;

var repoRoot = FindRepoRoot();
Console.WriteLine($"Repo root: {repoRoot}");

var config = "Release";
#if DEBUG
config = "Debug";
#endif

const string targetFrameworkVersion = "net10.0";
var pluginFrameworkVersionFile = Path.Combine(repoRoot, ".nuget.local", "PluginFramework.Version.props");

await RunDotnet(
    repoRoot,
    "build-server",
    "shutdown");

// Get PluginFramework version via probe project
var output = await RunDotnet(
    repoRoot,
    "pack",
    "src/PluginFramework/Laminar.PluginFramework.Version/Laminar.PluginFramework.Version.csproj " +
    $"-c {config} " +
    "/p:EmitPluginFrameworkVersion=true " +
    "/p:UseSharedCompilation=false")
    .ThrowOnError();

var pluginVersion = ExtractPluginFrameworkVersion(output.StdOut);
var persistentPluginVersion = await GetPersistentPluginFrameworkVersion();
if (!pluginVersion.Equals(persistentPluginVersion))
{
    Console.WriteLine($"PluginFramework version changed: {persistentPluginVersion} -> {pluginVersion}. Rebuilding and restoring...");
    
    // Build plugin framework first
    await RunDotnet(
        repoRoot,
        "pack",
        $"src/PluginFramework/Laminar.PluginFramework.SourceGeneration/Laminar.PluginFramework.SourceGeneration.csproj " +
        $"-c {config} " + 
        $"/p:PluginFrameworkVersion={pluginVersion} " +
        "/p:UseSharedCompilation=false").ThrowOnError();
    
    await RunDotnet(
            repoRoot,
            "pack",
            $"src/PluginFramework/Laminar.PluginFramework/Laminar.PluginFramework.csproj " +
            $"-c {config} " +
            $"/p:PluginFrameworkVersion={pluginVersion} " +
            "/p:UseSharedCompilation=false")
        .ThrowOnError();

    await SetPersistentPluginFrameworkVersion(pluginVersion);
    
    // Repo should be stable, restore to check:
    await RunDotnet(
            repoRoot,
            "restore",
            $"ProjectLaminar.slnx " +
            $"/p:PluginFrameworkVersion={pluginVersion} " +
            "/p:UseSharedCompilation=false")
        .ThrowOnError();
}


// Build plugins
await RunDotnet(
    repoRoot,
    "build",
    "src/Plugins/BasicFunctionality/BasicFunctionality.csproj " +
    $"-c {config} " +
    $"/p:PluginFrameworkVersion={pluginVersion} " +
    "/p:UseSharedCompilation=false")
    .ThrowOnError();

await RunDotnet(
    repoRoot,
    "build",
    "src/Plugins/BasicFunctionality.Avalonia/BasicFunctionality.Avalonia.csproj " +
    $"-c {config} " +
    $"/p:PluginFrameworkVersion={pluginVersion} " +
    "/p:UseSharedCompilation=false")
    .ThrowOnError();


// Build app
await RunDotnet(
    repoRoot,
    "build",
    $"src/Application/Laminar.Avalonia/Laminar.Avalonia.csproj " +
    $"-c {config} " +
    $"--no-restore " +
    $"/p:PluginFrameworkVersion={pluginVersion} " + 
    $"/p:UseSharedCompilation=false")
    .ThrowOnError();

// Load application assembly
var appPath = Path.Combine(repoRoot, "src", "Application", "Laminar.Avalonia", "bin", config, targetFrameworkVersion, "Laminar.Avalonia.dll");

if (!File.Exists(appPath))
    throw new FileNotFoundException(appPath);

Console.WriteLine($"Loading app: {appPath}");

AppContext.SetSwitch("System.Runtime.Loader.UsePathAssemblyResolver", true);
var loadContext = new ApplicationLoadContext(appPath);
var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(appPath)));

// Bootstrap and run
var bootstrapperType = assembly
    .GetTypes()
    .FirstOrDefault(t =>
        typeof(IApplicationBootstrapper).IsAssignableFrom(t)
        && t is { IsInterface: false, IsAbstract: false });

if (bootstrapperType is null)
    throw new InvalidOperationException("No application bootstrapper found");

var bootstrapper = (IApplicationBootstrapper)Activator.CreateInstance(bootstrapperType)!;
Console.WriteLine($"Bootstrapper '{bootstrapperType.FullName}' created. Running main app now");
await bootstrapper.Run(loadContext, args);

return;

static async Task<DotnetResult> RunDotnet(string repoRoot, string command, string args)
{
    Console.WriteLine($"> dotnet {command} {args}");

    var psi = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = $"{command} {args}",
        WorkingDirectory = repoRoot,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };

    using var process = Process.Start(psi)!;

    var stdoutTask = process.StandardOutput.ReadToEndAsync();
    var stderrTask = process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync();

    var stdout = await stdoutTask;
    var stderr = await stderrTask;

    return new DotnetResult(process.ExitCode, stdout, stderr, command);
}

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);

    while (dir is not null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            return dir.FullName;

        dir = dir.Parent;
    }

    return Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}

static string ExtractPluginFrameworkVersion(string buildOutput)
{
    const string prefix = "PLUGINFRAMEWORK_VERSION=";

    var line = buildOutput
        .Split('\n')
        .Select(l => l.Trim())
        .FirstOrDefault(l => l.Contains(prefix));

    if (line is null)
        throw new Exception("PluginFramework version not emitted");

    return line[(line.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length)..].Trim();
}

async Task SetPersistentPluginFrameworkVersion(string version)
{
    var tempFile = pluginFrameworkVersionFile + ".tmp";
    var contents =
        $"""
        <Project>
            <PropertyGroup Condition="'$(PluginFrameworkVersion)' == ''">
                <PluginFrameworkVersion>{version}</PluginFrameworkVersion>
            </PropertyGroup>
        </Project>                               
        """;

    await File.WriteAllTextAsync(tempFile, contents);
    File.Move(tempFile, pluginFrameworkVersionFile, overwrite: true);
}

async Task<string?> GetPersistentPluginFrameworkVersion()
{
    if (!File.Exists(pluginFrameworkVersionFile))
    {
        return null;
    }

    var contents = await File.ReadAllTextAsync(pluginFrameworkVersionFile);
    int openingTagPosition = contents.IndexOf("<PluginFrameworkVersion>", StringComparison.Ordinal) + "<PluginFrameworkVersion>".Length;
    int closingTagPosition = contents.IndexOf("</PluginFrameworkVersion", StringComparison.Ordinal);
    return contents.Substring(openingTagPosition, closingTagPosition - openingTagPosition).Trim();
}

internal record DotnetResult(int ExitCode, string StdOut, string StdErr, string Command);

internal static class DotnetResultHelpers
{
    extension(Task<DotnetResult> resultTask)
    {
        public async Task<DotnetResult> ThrowOnError()
        {
            var result = await resultTask;

            if (result.ExitCode != 0)
            {
                await Console.Error.WriteLineAsync(result.StdErr);
                throw new Exception($"dotnet {result.Command} failed. Exit code: {result.ExitCode}");
            }

            return result;
        }
    }
}

internal sealed class ApplicationLoadContext(string mainAssemblyPath) : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver = new(mainAssemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is "Bootstrapping")
        {
            return null;
        }
        
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}