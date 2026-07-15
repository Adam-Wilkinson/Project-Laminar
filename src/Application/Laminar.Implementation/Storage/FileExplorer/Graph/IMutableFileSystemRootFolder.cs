namespace Laminar.Implementation.Storage.FileExplorer.Graph;

internal interface IMutableFileSystemRootFolder : IMutableFileSystemFolder
{
    public void OnRemoved(FileSystemGraph.MutationToken _, bool cleanupInfoFiles);
}