using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

public interface IFileSystemItemRepository
{
    public bool TryGetOutdated(FileSystemPath outdatedPath, [NotNullWhen(true)] out IFileSystemItem? item);

    public void ClearOutdated();
    
    public bool TryGetExisting(FileSystemPath path, [NotNullWhen(true)] out IFileSystemItem? item);
}