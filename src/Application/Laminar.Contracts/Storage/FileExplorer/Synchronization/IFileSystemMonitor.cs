using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Synchronization;

public interface IFileSystemMonitor
{
    IDisposable StartMonitoring(IFileSystemRootFolder folder, FileSystemPath[]? excludedPaths = null);
}
