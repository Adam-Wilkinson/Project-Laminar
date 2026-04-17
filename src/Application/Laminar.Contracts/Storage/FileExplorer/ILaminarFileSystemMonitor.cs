namespace Laminar.Contracts.Storage.FileExplorer;

public interface ILaminarFileSystemMonitor
{
    IDisposable StartMonitoring(ILaminarStorageRootFolder folder);
}
