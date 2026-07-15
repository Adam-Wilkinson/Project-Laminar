using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;

namespace Laminar.Implementation.Storage.FileExplorer.Graph;

internal interface IMutableFileSystemItemRepository : IFileSystemItemRepository
{
    public void Add(FileSystemGraph.MutationToken _, IFileSystemItem item);

    public void Remove(FileSystemGraph.MutationToken _, IFileSystemItem item);
}