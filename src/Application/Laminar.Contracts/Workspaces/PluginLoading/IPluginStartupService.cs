using System.Runtime.Loader;
using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Workspaces.PluginLoading;

public interface IPluginStartupService
{
    public Task Initialize(FrontendDependency frontend, AssemblyLoadContext? defaultLoadContext);
}