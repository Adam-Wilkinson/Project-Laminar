using System.Runtime.Loader;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface ISharedPluginContext
{
    public AssemblyLoadContext DefaultLoadContext { get; }
    
    public FrontendDependency FrontendDependency { get; }
    
    public Platforms CurrentPlatform { get; }

    public FileSystemPath OfflineCacheLocation { get; }

    public void Configure(FrontendDependency frontendDependency, Platforms currentPlatform, AssemblyLoadContext? assemblyLoadContext);
    
    public IReadOnlyList<IInstalledPlugin> GetInstallations(IPluginInfo pluginInfo);
    
    public void RegisterInstallation(IInstalledPlugin installedPlugin); 
}