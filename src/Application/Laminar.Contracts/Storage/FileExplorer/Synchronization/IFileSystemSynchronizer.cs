namespace Laminar.Contracts.Storage.FileExplorer.Synchronization;

public interface IFileSystemSynchronizer
{
    public void OnFileSystemEvent(FileSystemEvent e);

    public void ReconcileAndReset(IReadOnlyCollection<IFileSystemFolder> targetFolders);
}