using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.Infrastructure;

internal sealed class FileSystemEventHashBucket(
    IFileSystemItemHasher hasher,
    IFileSystem fileSystem,
    IFileSystemItemRepository repository) : IFileSystemEventHashBucket
{
    private readonly Dictionary<int, ItemHashCodeInfo> _hashInfos = [];

    public bool TryGetInfoForPath(FileSystemPath? path, out ItemHashCodeInfo info)
    {
        if (!TryHashByPath(path, out var hash))
        {
            info = default;
            return false;
        }
            
        return _hashInfos.TryGetValue(hash, out info);
    }
    
    public void AddEvent(FileSystemEvent fileSystemEvent) 
    {
        if (!TryHashByPath(fileSystemEvent.NewPath, out var hash))
        {
            return;
        }

        if (_hashInfos.TryGetValue(hash, out var hashInfo))
        {
            hashInfo.State = ItemHashCodeState.Clash;
            hashInfo.Event = null;
            return;
        }

        _hashInfos[hash] = new ItemHashCodeInfo
        {
            Event = fileSystemEvent,
            Hash = hash,
            State = ItemHashCodeState.Single,
        };
    }
    
    private bool TryHashByPath(FileSystemPath? path, out int hash)
    {
        if (!path.HasValue)
        {
            hash = -1;
            return false;
        }
        
        if (TryResolveItem(path.Value, out var item) && hasher.TryHashItem(item, path, out int itemHash))
        {
            hash = itemHash;
            return true;
        }

        if (fileSystem.Exists(path.Value))
        {
            hash = hasher.HashFromPath(path.Value);
            return true;
        }

        hash = -1;
        return false;
    }
    
    private bool TryResolveItem(FileSystemPath path, [NotNullWhen(true)] out IFileSystemItem? item)
    {
        if (repository.TryGetOutdated(path, out var outdatedItem))
        {
            item = outdatedItem;
            return true;
        }

        if (repository.TryGetExisting(path, out var existing))
        {
            item = existing;
            return true;
        }

        item = null;
        return false;
    }
}