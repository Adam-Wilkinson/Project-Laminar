using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

public interface IFileSystemRoots : IReadOnlyObservableCollection<IFileSystemRootFolder>
{
    public IFileSystemRootFolder AddRoot(FileSystemPath path);

    public bool RemoveRootAt(FileSystemPath path, bool deleteInfoFiles);

    public bool RemoveRootAt(FileSystemPath path, bool deleteInfoFiles, out IFileSystemRootFolder? oldRoot);
}