using Laminar.Domain;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInstaller
{
    public Task<MayError<IPluginInstallation>> InstallFromArchive(
        Stream archiveStream, 
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost);
    
    public Task<MayError<IPluginInstallation>> InstallFromFolder(
        FileSystemPath directory, 
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost);
}