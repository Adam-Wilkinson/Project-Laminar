using Laminar.Domain;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInstaller
{
    public Task<MayError<IInstalledPlugin>> InstallFromArchive(
        Stream archiveStream, 
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost);
    
    public Task<MayError<IInstalledPlugin>> InstallFromFolder(
        FileSystemPath directory, 
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost);
}