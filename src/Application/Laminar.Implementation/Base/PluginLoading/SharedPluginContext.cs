using System.Runtime.Loader;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class SharedPluginContext : ISharedPluginContext
{
    private readonly Dictionary<string, List<IInstalledPlugin>> _installedPlugins = [];
    
    private bool _configured;
    private AssemblyLoadContext? _assemblyLoadContext;
    private FrontendDependency? _frontendDependency;
    private Platforms? _platforms;
    
    public AssemblyLoadContext DefaultLoadContext => _assemblyLoadContext ?? throw new InvalidOperationException("Plugin install context is not configured");
    
    public FrontendDependency FrontendDependency =>  _frontendDependency ?? throw new InvalidOperationException("Plugin install context is not configured");
    
    public Platforms CurrentPlatform =>  _platforms ?? throw new InvalidOperationException("Plugin install context is not configured");

    public FileSystemPath OfflineCacheLocation { get; } = DataLocations.LocalDataFolder.ChildPath("Plugins");

    public void Configure(FrontendDependency frontendDependency, Platforms currentPlatform,
        AssemblyLoadContext? assemblyLoadContext)
    {
        if (_configured) throw new InvalidOperationException("Plugin install context is already configured");
        
        _assemblyLoadContext = assemblyLoadContext ?? AssemblyLoadContext.Default;
        _frontendDependency = frontendDependency;
        _platforms = currentPlatform;
        
        _configured = true;
    }

    public IReadOnlyList<IInstalledPlugin> GetInstallations(string pluginId)
        =>  _installedPlugins.TryGetValue(pluginId, out var list) ? list : Array.Empty<IInstalledPlugin>();

    public void RegisterInstallation(IInstalledPlugin installedPlugin)
    {
        if (_installedPlugins.TryGetValue(installedPlugin.PluginId.Name, out var installation))
        {
            installation.Add(installedPlugin);
        }
        
        installation = [installedPlugin];
        _installedPlugins[installedPlugin.PluginId.Name] = installation;
    }
}