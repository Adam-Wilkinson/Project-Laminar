using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.Synchronization;

internal sealed class FileSystemEventHashBucket(
    IOutdatedItemsBuffer? outdatedFileLocations,
    IFileSystemItemHasher hasher,
    IFileSystem fileSystem,
    IFileSystemItemRepository repository) : IFileSystemEventHashBucket
{
    private readonly Dictionary<int, HashBucketInfo> _hashInfos = [];

    public bool TryGetInfoForPath(FileSystemPath? path, out HashBucketInfo info)
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

        if (_hashInfos.TryGetValue(hash, out _))
        {
            _hashInfos[hash] = new HashBucketInfo
            {
                Hash = hash,
                Event = null,
                State = HashBucketState.Clash,
            };
            return;
        }

        _hashInfos[hash] = new HashBucketInfo
        {
            Event = fileSystemEvent,
            Hash = hash,
            State = HashBucketState.Single,
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
        if (outdatedFileLocations is not null && outdatedFileLocations.TryGetItem(path, out var outdatedItem))
        {
            item = outdatedItem;
            return true;
        }

        if (repository.TryGetItem(path, out var existing))
        {
            item = existing;
            return true;
        }

        item = null;
        return false;
    }
}