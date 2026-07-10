using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

public interface IFileSystemMutationComputer
{
    public IEnumerable<FileSystemGraphMutation> ComputeMutationsAndClear();
    
    public void AddEvent(FileSystemEvent fileSystemEvent);
}

public struct FileSystemEvent
{
    public static FileSystemEvent Created(FileSystemPath path) => new(WatcherChangeTypes.Created, null, path);
    
    public static FileSystemEvent Deleted(FileSystemPath path) => new(WatcherChangeTypes.Deleted, path, null);
    
    public static FileSystemEvent Renamed(FileSystemPath oldPath, FileSystemPath newPath) => new(WatcherChangeTypes.Renamed, oldPath, newPath);
    
    public static FileSystemEvent Changed(FileSystemPath path) => new(WatcherChangeTypes.Changed, path, path);
    
    private FileSystemEvent(WatcherChangeTypes changeType, FileSystemPath? oldPath, FileSystemPath? newPath)
    {
        ChangeType = changeType;
        OldPath = oldPath;
        NewPath = newPath;
    }
    
    public WatcherChangeTypes ChangeType { get; }
    
    public FileSystemPath? OldPath { get; }
    
    public FileSystemPath? NewPath { get; }
}