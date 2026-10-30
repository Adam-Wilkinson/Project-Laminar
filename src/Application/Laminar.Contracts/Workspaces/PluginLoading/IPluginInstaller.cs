using Laminar.Domain;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Workspaces.PluginLoading;

public interface IPluginInstaller
{
    public Task<MayError<IPluginInstallation>> InstallFromArchive(
        Stream archiveStream, 
        VersionedPluginId pluginId,
        IWorkspace workspace);
    
    public Task<MayError<IPluginInstallation>> InstallFromFolder(
        FileSystemPath directory, 
        VersionedPluginId pluginId,
        IWorkspace workspace);
}