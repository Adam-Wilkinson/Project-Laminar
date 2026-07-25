using System.Diagnostics.CodeAnalysis;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Graph;

public interface IFileSystemItemRepository
{
    public IOutdatedItemsBuffer DetachOutdatedItems();
    
    public bool TryGetItem(FileSystemPath path, [NotNullWhen(true)] out IFileSystemItem? item);
}