using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Synchronization;

public interface IFileSystemItemHasher
{
    public bool TryHashItem(IFileSystemItem item, FileSystemPath? pathOverride, out int hash);

    public int HashFromPath(FileSystemPath path);
}