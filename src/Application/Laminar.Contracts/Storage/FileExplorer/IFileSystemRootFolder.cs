using Laminar.Contracts.Workspaces;

namespace Laminar.Contracts.Storage.FileExplorer;

public interface IFileSystemRootFolder : IFileSystemFolder
{
    public IWorkspace Workspace { get; }
}