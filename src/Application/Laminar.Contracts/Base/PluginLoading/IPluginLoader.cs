using System.Runtime.Loader;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginLoader
{
    public IEnumerable<IRegisteredPlugin> LoadFrom(
        FileSystemPath pluginPath, 
        FrontendDependency frontendDependency, 
        AssemblyLoadContext defaultLoadContext);
}