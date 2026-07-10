using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

public interface IFileSystemGraphMutator
{
    public void Apply(FileSystemGraphMutation mutation, IFileSystemGraph graph);
}

public record struct FileSystemGraphMutation(GraphMutationType EventType, FileSystemPath? OldPath, FileSystemPath? NewPath);
    
public enum GraphMutationType
{
    Created,
    Deleted,
    Renamed,
    Moved
}