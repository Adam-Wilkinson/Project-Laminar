using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Exceptions;

public class DependentPluginNotFoundException(VersionedPluginId plugin, VersionedPluginId dependency) : Exception($"Could not install plugin {plugin} because the dependency {dependency} could not be found")
{
    public VersionedPluginId Plugin => plugin;
    
    public VersionedPluginId Dependency => dependency;
}