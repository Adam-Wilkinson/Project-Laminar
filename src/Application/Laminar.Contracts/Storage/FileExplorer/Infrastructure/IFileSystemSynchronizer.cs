namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

public interface IFileSystemSynchronizer
{
    public void OnFileSystemEvent(FileSystemEvent e);

    public void ReconcileAndReset(IReadOnlyCollection<IFileSystemRootFolder> targetFolders);
}