namespace Laminar.Contracts.Storage.FileExplorer;

public interface IFileSystemRootFolder : IFileSystemFolder, IDisposable
{
    public void Dispose(bool cleanupInfoFiles);
}