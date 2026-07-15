using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Graph;

public interface IFileSystemGraphMutator
{
    public void Apply(FileSystemGraphMutation mutation, IFileSystemGraph graph);
}

public record struct FileSystemGraphMutation(FileSystemGraphMutationType Type, FileSystemPath? OldPath, FileSystemPath? NewPath);
    
public enum FileSystemGraphMutationType
{
    Creation,
    Deletion,
    Rename,
    Move
}