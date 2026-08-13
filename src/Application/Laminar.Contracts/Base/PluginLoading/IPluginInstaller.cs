using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInstaller
{
    public Task<IInstalledPlugin> InstallFromArchive(
        Stream archiveStream, 
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost);
    
    public Task<IInstalledPlugin> InstallFromFolder(
        FileSystemPath directory, 
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost);
}