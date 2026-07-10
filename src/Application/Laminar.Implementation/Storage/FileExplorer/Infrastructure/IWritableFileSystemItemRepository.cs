using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;

namespace Laminar.Implementation.Storage.FileExplorer.Infrastructure;

internal interface IWritableFileSystemItemRepository : IFileSystemItemRepository
{
    public void Add(FileSystemGraph.MutationToken _, IFileSystemItem item);

    public void Remove(FileSystemGraph.MutationToken _, IFileSystemItem item);
}