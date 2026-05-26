using System.Reflection;
using Bootstrapping;
using Laminar.Build;
using Laminar.Runner;

const string targetFrameworkVersion = "net10.0";

await LaminarBuilder.Build();

// Load application assembly
var appPath = Path.Combine(Dotnet.RepoRoot, "src", "Application", "Laminar.Avalonia", "bin", Dotnet.BuildConfig, targetFrameworkVersion, "Laminar.Avalonia.dll");

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
try
{
    await bootstrapper.Run(loadContext, args);
}
finally
{
    var weakRef = new WeakReference(loadContext);

    loadContext.Unload();

    for (var i = 0; weakRef.IsAlive && i < 10; i++)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    if (weakRef.IsAlive)
    {
        Console.WriteLine("Bootstrapped project is still running after application close");
    }
}