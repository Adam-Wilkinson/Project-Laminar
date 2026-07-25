using Laminar.Contracts.Storage.FileExplorer;

namespace Laminar.Implementation.Storage.FileExplorer.Graph;

internal interface IMutableFileSystemItem : IFileSystemItem
{
    internal void SetNameInternal(FileSystemGraph.MutationToken _, string newNameWithExtension);
    
    internal void SetParentInternal(FileSystemGraph.MutationToken _, IFileSystemFolder newParent);
    
    internal void OnDeleted();
}