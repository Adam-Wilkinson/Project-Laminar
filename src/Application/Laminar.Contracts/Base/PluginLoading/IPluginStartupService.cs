using System.Runtime.Loader;
using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginStartupService
{
    public void Initialize(FrontendDependency frontend, AssemblyLoadContext? defaultLoadContext);
}