using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class DeletedStorageItemCache(IFileSystem fileSystem) : IDeletedStorageItemCache
{
    private static readonly TimeSpan DeletedItemMoveDetectionCooldown = new(0, 0, 2);

    private readonly Dictionary<int, StorageItemDescriptorBucket> _cache = [];

    public void RegisterPotentialDeletion(ILaminarStorageItem potentialDeletion)
    {
        var descriptor = StorageItemDescriptor.FromItem(potentialDeletion);

        if (descriptor is null)
            return;

        int hash = ComputeHash(descriptor.Value);

        if (!_cache.TryGetValue(hash, out var bucket))
        {
            bucket = new StorageItemDescriptorBucket();
            _cache[hash] = bucket;
        }

        bucket.Add(descriptor.Value, potentialDeletion);
    }

    public ILaminarStorageItem? TryFindAndRemove(FileSystemPath path)
    {
        if (!fileSystem.Exists(path)) return null;
        
        var descriptor = StorageItemDescriptor.FromPath(path, fileSystem);

        int hash = ComputeHash(descriptor);

        if (!_cache.TryGetValue(hash, out var bucket))
            return null;
        
        return bucket.TryGetAndPop(descriptor, out var item) ? item : null;
    }

    public void CommitDeletions()
    {
        while (_cache.Count > 0)
        {
            var (key, value) = _cache.First();
            _cache.Remove(key);
            value.DeleteAllContents();
        }
    }
    
    private static int ComputeHash(StorageItemDescriptor d)
    {
        var hash = new HashCode();

        hash.Add(d.Name, FileSystemPath.RuntimeStringComparer);
        hash.Add(d.IsFolder);

        if (!d.IsFolder)
        {
            hash.Add(d.Size);
            return hash.ToHashCode();
        }

        if (d.Children is null)
        {
            // Not eligible / weak descriptor
            return hash.ToHashCode();
        }

        int fileCount = 0;
        int folderCount = 0;

        foreach (var child in d.Children)
        {
            hash.Add(child.Name, FileSystemPath.RuntimeStringComparer);
            hash.Add(child.IsFolder);

            if (child.IsFolder)
            {
                folderCount++;
            }
            else
            {
                fileCount++;
                hash.Add(child.Size);
            }
        }

        hash.Add(fileCount);
        hash.Add(folderCount);

        return hash.ToHashCode();
    }


    private static bool DescriptorsAreEqual(StorageItemDescriptor a, StorageItemDescriptor b)
    {
        if (!FileSystemPath.RuntimeStringComparer.Equals(a.Name, b.Name))
            return false;

        if (a.IsFolder != b.IsFolder)
            return false;

        if (!a.IsFolder)
            return a.Size == b.Size;

        // Folder comparison
        if (a.Children is null || b.Children is null)
            return false; // not strong enough → refuse match

        // Counts must match
        if (a.Children.Count() != b.Children.Count())
            return false;

        using var enumA = a.Children.GetEnumerator();
        using var enumB = b.Children.GetEnumerator();

        while (enumA.MoveNext() && enumB.MoveNext())
        {
            var ca = enumA.Current;
            var cb = enumB.Current;

            if (!FileSystemPath.RuntimeStringComparer.Equals(ca.Name, cb.Name))
                return false;

            if (ca.IsFolder != cb.IsFolder)
                return false;

            if (!ca.IsFolder && ca.Size != cb.Size)
                return false;
        }

        return true;
    }
    
    private class StorageItemDescriptorBucket
    {
        private readonly List<(StorageItemDescriptor descriptor, ILaminarStorageItem item, DateTime timestamp)> _values = [];

        public void Add(StorageItemDescriptor descriptor, ILaminarStorageItem item)
        {
            _values.Add((descriptor, item, DateTime.Now));
        }

        public bool TryGetAndPop(StorageItemDescriptor descriptor, [NotNullWhen(true)] out ILaminarStorageItem? item)
        {
            int hitIndex = _values.Index()
                .FirstOrDefault(x => DateTime.Now - x.Item.timestamp < DeletedItemMoveDetectionCooldown && DescriptorsAreEqual(descriptor, x.Item.descriptor))
                is var match
                ? match.Index : -1;

            if (hitIndex == -1)
            {
                item = null;
                return false;
            }

            item = _values[hitIndex].item;
            _values.RemoveAt(hitIndex);
            return true;
        }

        public void DeleteAllContents()
        {
            foreach (var (_, item, _) in _values)
            {
                (item as LaminarStorageItem)?.RaiseOnDeleted();
            }
            
            _values.Clear();
        }
    }
}

internal readonly struct StorageItemDescriptor
{
    private static readonly IComparer<StorageItemDescriptor> Comparer = Comparer<StorageItemDescriptor>.Create(
        (x, y) => FileSystemPath.RuntimeStringComparer.Compare(x.Name, y.Name));
    
    public string Name { get; private init; }
    
    public bool IsFolder { get; private init; }
    
    // For files
    public long? Size { get; private init; }
    
    // For folders
    public IEnumerable<StorageItemDescriptor>? Children { get; private init; }

    public static StorageItemDescriptor FromPath(FileSystemPath path, IFileSystem fileSystem)
    {
        bool isFolder = fileSystem.IsDirectory(path);
        
        return new()
        {
            Name = path.NameAndExtension,
            IsFolder = isFolder,
            Size = isFolder ? null : fileSystem.GetFileSize(path),
            Children = isFolder
                ? fileSystem.EnumerateChildren(path)
                    .Select(child =>
                    {
                        var childIsFolder = fileSystem.IsDirectory(child);
                        return new StorageItemDescriptor
                        {
                            Name = child.NameAndExtension,
                            IsFolder = childIsFolder,
                            Size = childIsFolder ? null : fileSystem.GetFileSize(child),
                        };
                    })
                    .ToImmutableSortedSet(Comparer)
                : null
        };
    }

    public static StorageItemDescriptor? FromItem(ILaminarStorageItem item) => item switch
    {
        ILaminarStorageFile file => new()
        {
            Name = file.Path.NameAndExtension,
            IsFolder = false,
            Size = file.SizeOnDisk,
        },
        LaminarStorageFolder { ContentsIsInitialized: true } folder => new()
        {
            Name = folder.Path.NameAndExtension,
            IsFolder = true,
            Children = folder.Contents
                .Select(child => new StorageItemDescriptor
                {
                    Name = child.Path.NameAndExtension,
                    IsFolder = child is ILaminarStorageFolder,
                    Size = child is ILaminarStorageFile file ? file.SizeOnDisk : null,
                })
                .ToImmutableSortedSet(Comparer)
        },
        _ => null
    };
}