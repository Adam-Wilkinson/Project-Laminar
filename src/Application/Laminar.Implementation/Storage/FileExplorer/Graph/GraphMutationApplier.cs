using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.IO;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer.Graph;

internal sealed class GraphMutationApplier(
    IFileSystem fileSystem, 
    IFileSystemItemRepository repository,
    ILogger<GraphMutationApplier> logger) : IGraphMutationApplier
{
    public void Apply(FileSystemGraphMutation mutation, IFileSystemGraph graph)
    {
        switch (mutation.Type)
        {
            case FileSystemGraphMutationType.Move when repository.TryGetItem(mutation.OldPath!.Value, out var item):
                if (mutation.NewPath?.Parent is not { } newParentPath
                    || !repository.TryGetItem(newParentPath, out var newParent)
                    || newParent is not IFileSystemFolder newParentFolder)
                {
                    logger.LogWarning("Move is unable to be reflected by move event, unable to find new parent item");
                    break;
                }
            
                if (newParentFolder.GetOrLoadContents().Contains(item))
                {
                    break;
                } 
                    
                graph.Move(item, newParentFolder, 0);
                break;   
                
            case FileSystemGraphMutationType.Creation:
                if (mutation.NewPath?.Parent is not { } createdPathParent
                    || !repository.TryGetItem(createdPathParent, out var parentOfCreatedItem)
                    || parentOfCreatedItem is not IFileSystemFolder parentFolder)
                {
                    logger.LogWarning("Unable to create storage item model at {path} because parent folder could not be found in existing item repository", mutation.NewPath);
                    break;
                }
            
                if (parentFolder.GetOrLoadContents().Any(x => x.Path == mutation.NewPath.Value))
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
                
            case FileSystemGraphMutationType.Deletion:
                if (repository.TryGetItem(mutation.OldPath!.Value, out var deletedItem))
                {
                    graph.Remove(deletedItem);
                }
            
                break;
                
            case FileSystemGraphMutationType.Rename:
                if (repository.TryGetItem(mutation.OldPath!.Value, out var renamedItem))
                {
                    graph.Rename(renamedItem, mutation.NewPath!.Value.NameAndExtension);
                }
            
                break;
        }
    }
}