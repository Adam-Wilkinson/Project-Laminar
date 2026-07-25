using Laminar.Contracts.Storage.FileExplorer;

namespace Laminar.Implementation.Storage.FileExplorer.Graph;

internal interface IMutableFileSystemRootFolder : IMutableFileSystemFolder, IFileSystemRootFolder
{
    public void OnRemoved(FileSystemGraph.MutationToken _, bool cleanupInfoFiles);
}