using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer.Infrastructure;

internal sealed class FileSystemGraphMutator(
    IFileSystem fileSystem, 
    IFileSystemItemRepository repository,
    ILogger<FileSystemGraphMutator> logger) : IFileSystemGraphMutator
{
    public void Apply(FileSystemGraphMutation mutation, IFileSystemGraph graph)
    {
        switch (mutation.EventType)
        {
            case GraphMutationType.Moved when repository.TryGetExisting(mutation.OldPath!.Value, out var item):
                if (mutation.NewPath?.Parent is not { } newParentPath
                    || !repository.TryGetExisting(newParentPath, out var newParent)
                    || newParent is not IFileSystemFolder newParentFolder)
                {
                    logger.LogWarning("Move is unable to be reflected by move event, unable to find new parent item");
                    break;
                }
            
                if (newParentFolder.LoadOrGetContents().Contains(item))
                {
                    break;
                } 
                    
                graph.Move(item, newParentFolder, 0);
                break;   
                
            case GraphMutationType.Created:
                if (mutation.NewPath?.Parent is not { } createdPathParent
                    || !repository.TryGetExisting(createdPathParent, out var parentOfCreatedItem)
                    || parentOfCreatedItem is not IFileSystemFolder parentFolder)
                {
                    logger.LogWarning("Unable to create storage item model at {path} because parent folder could not be found in existing item repository", mutation.NewPath);
                    break;
                }
            
                if (parentFolder.LoadOrGetContents().Any(x => x.Path == mutation.NewPath.Value))
                {
                    break;
                }
                    
                if (fileSystem.IsDirectory(mutation.NewPath.Value))
                {
                    graph.AddFolder(parentFolder, 0, mutation.NewPath.Value.NameAndExtension);
                }
                else
                {
                    graph.AddFile(parentFolder, 0, mutation.NewPath.Value.NameAndExtension);
                }
            
                break;
                
            case GraphMutationType.Deleted:
                if (repository.TryGetExisting(mutation.OldPath!.Value, out var deletedItem))
                {
                    graph.Delete(deletedItem);
                }
            
                break;
                
            case GraphMutationType.Renamed:
                if (repository.TryGetExisting(mutation.OldPath!.Value, out var renamedItem))
                {
                    graph.Rename(renamedItem, mutation.NewPath!.Value.NameAndExtension);
                }
            
                break;
        }
    }
}