using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Synchronization;

public interface IFileSystemEventHashBucket
{
    public bool TryGetInfoForPath(FileSystemPath? path, out HashBucketInfo info);

    public void AddEvent(FileSystemEvent fileSystemEvent);
}

public struct HashBucketInfo
{
    public FileSystemEvent? Event { get; set; }
        
    public int Hash { get; init; }
        
    public HashBucketState State { get; set; }
}

public enum HashBucketState
{
    None,
    Single,
    Clash,
}