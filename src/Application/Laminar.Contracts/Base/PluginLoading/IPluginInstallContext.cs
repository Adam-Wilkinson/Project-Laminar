using System.Runtime.Loader;
using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInstallContext
{
    public void Configure(FrontendDependency frontendDependency, Platforms currentPlatform, AssemblyLoadContext? assemblyLoadContext);
    public AssemblyLoadContext DefaultLoadContext { get; }
    public FrontendDependency FrontendDependency { get; }
    public Platforms CurrentPlatform { get; }
}