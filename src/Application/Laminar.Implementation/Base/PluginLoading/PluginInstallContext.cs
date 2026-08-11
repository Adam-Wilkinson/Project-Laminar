using System.Runtime.Loader;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginInstallContext : IPluginInstallContext
{
    private bool _configured;
    private AssemblyLoadContext? _assemblyLoadContext;
    private FrontendDependency? _frontendDependency;
    private Platforms? _platforms;
    
    public void Configure(FrontendDependency frontendDependency, Platforms currentPlatform,
        AssemblyLoadContext? assemblyLoadContext)
    {
        if (_configured) throw new InvalidOperationException("Plugin install context is already configured");
        
        _assemblyLoadContext = assemblyLoadContext ?? AssemblyLoadContext.Default;
        _frontendDependency = frontendDependency;
        _platforms = currentPlatform;
        
        _configured = true;
    }

    public AssemblyLoadContext DefaultLoadContext => _assemblyLoadContext ?? throw new InvalidOperationException("Plugin install context is not configured");
    
    public FrontendDependency FrontendDependency =>  _frontendDependency ?? throw new InvalidOperationException("Plugin install context is not configured");
    
    public Platforms CurrentPlatform =>  _platforms ?? throw new InvalidOperationException("Plugin install context is not configured");
}