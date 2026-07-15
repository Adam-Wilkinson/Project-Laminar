using Laminar.Contracts.Storage.FileExplorer;

namespace Laminar.Implementation.Storage.FileExplorer.Graph;

internal interface IMutableFileSystemFolder : IMutableFileSystemItem
{
    internal void InsertChildInternal(FileSystemGraph.MutationToken _, IFileSystemItem newChild, int index);
    
    internal void RemoveChildInternal(FileSystemGraph.MutationToken _, IFileSystemItem child);
    
    internal void MoveChildInternal(FileSystemGraph.MutationToken _, int oldIndex, int newIndex);
}