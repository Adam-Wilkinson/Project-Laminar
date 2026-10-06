using Laminar.Domain;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInstaller
{
    public Task<MayError<bool>> InstallFromArchive(
        Stream archiveStream, 
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost);
    
    public Task<MayError<bool>> InstallFromFolder(
        FileSystemPath directory, 
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost);
}