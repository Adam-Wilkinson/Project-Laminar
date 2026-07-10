using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.Extensions;

namespace Laminar.Implementation.Storage.FileExplorer.Infrastructure;

internal sealed class FileSystemDiscrepancyComputer(
    IFileSystem fileSystem) : IFileSystemDiscrepancyComputer
{
    public IEnumerable<FileSystemEvent> ComputeFolderDiscrepancies(IFileSystemFolder folder)
        => ComputeDiscrepanciesInternal(folder);

    private IEnumerable<FileSystemEvent> ComputeDiscrepanciesInternal(IFileSystemItem item)
    {
        if (!fileSystem.Exists(item.Path))
        {
            return FileSystemEvent.Deleted(item.Path).Yield();
        }
        
        if (item is IFileSystemFile || item is not IFileSystemFolder { Contents: not null } initializedFolder)
        {
            return [];
        }

        List<FileSystemEvent> changes = [];

        changes.AddRange(fileSystem.EnumerateChildren(initializedFolder.Path)
            .Where(childPath => initializedFolder.Contents.All(child => child.Path != childPath))
            .Select(FileSystemEvent.Created));

        foreach (var child in initializedFolder.Contents)
        {
            changes.AddRange(ComputeDiscrepanciesInternal(child));
        }

        return changes;
    }
}