using Laminar.Contracts.Base.PluginLoading;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface IFileSystemRootFolder : IFileSystemFolder
{
    public IWorkspace Workspace { get; }
}