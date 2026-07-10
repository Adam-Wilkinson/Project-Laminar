using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

public interface IFileSystemMonitor
{
    IDisposable StartMonitoring(IFileSystemRootFolder folder, FileSystemPath[]? excludedPaths = null);
}
