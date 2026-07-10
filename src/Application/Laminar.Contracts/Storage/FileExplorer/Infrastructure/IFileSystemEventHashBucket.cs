using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.FileExplorer.Infrastructure;

public interface IFileSystemEventHashBucket
{
    public bool TryGetInfoForPath(FileSystemPath? path, out ItemHashCodeInfo info);

    public void AddEvent(FileSystemEvent fileSystemEvent);
}

public struct ItemHashCodeInfo
{
    public FileSystemEvent? Event { get; set; }
        
    public int Hash { get; init; }
        
    public ItemHashCodeState State { get; set; }
}

public enum ItemHashCodeState
{
    None,
    Single,
    Clash,
}