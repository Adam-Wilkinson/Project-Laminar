using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginInstaller
{
    public Task<IInstalledPlugin> InstallFromArchive(
        Stream archiveStream, 
        IPluginInfo pluginInfo,
        SemanticVersion version,
        IRuntimeHost runtimeHost);
    
    public Task<IInstalledPlugin> InstallFromFolder(
        FileSystemPath directory, 
        IPluginInfo pluginInfo, 
        SemanticVersion version,
        IRuntimeHost runtimeHost);
}