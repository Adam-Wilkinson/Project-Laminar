using System.Runtime.Loader;

namespace Bootstrapping;

public interface IApplicationBootstrapper
{
    public Task Run(AssemblyLoadContext defaultLoadContext, string[] args);
}