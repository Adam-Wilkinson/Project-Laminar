using System.Diagnostics.CodeAnalysis;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Graph;

public interface IFileSystemItemRepository
{
    public bool TryGetOutdated(FileSystemPath outdatedPath, [NotNullWhen(true)] out IFileSystemItem? item);

    public void ClearOutdated();
    
    public bool TryGetExisting(FileSystemPath path, [NotNullWhen(true)] out IFileSystemItem? item);
}