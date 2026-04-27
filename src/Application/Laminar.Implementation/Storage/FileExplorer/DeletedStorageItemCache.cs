using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class DeletedStorageItemCache(IFileSystem fileSystem) : IDeletedStorageItemCache
{
    private static readonly TimeSpan DeletedItemMoveDetectionCooldown = new(0, 0, 2);

    private readonly Dictionary<int, List<(StorageItemDescriptor descriptor, ILaminarStorageItem item, DateTime timestamp)>> _cache = [];

    public void RegisterPotentialDeletion(ILaminarStorageItem potentialDeletion)
    {
        var descriptor = StorageItemDescriptor.FromItem(potentialDeletion);

        // Not eligible (e.g. folder without initialized contents)
        if (descriptor is null)
            return;

        int hash = ComputeHash(descriptor.Value);

        if (!_cache.TryGetValue(hash, out var bucket))
        {
            bucket = [];
            _cache[hash] = bucket;
        }

        bucket.Add((descriptor.Value, potentialDeletion, DateTime.Now));
    }

    public ILaminarStorageItem? TryFind(FileSystemPath path)
    {
        var descriptor = StorageItemDescriptor.FromPath(path, fileSystem);

        // If descriptor has no children for a folder, you still allow it,
        // but matching will naturally fail unless it’s strong enough.
        int hash = ComputeHash(descriptor);

        if (!_cache.TryGetValue(hash, out var bucket))
            return null;

        var now = DateTime.Now;

        foreach (var (cachedDescriptor, item, timestamp) in bucket)
        {
            if (now - timestamp > DeletedItemMoveDetectionCooldown)
                continue;

            if (DescriptorsAreEqual(cachedDescriptor, descriptor))
                return item;
        }

        return null;
    }

    public void Clear() => _cache.Clear();

    // -------------------------
    // Hashing (fast pre-filter)
    // -------------------------

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

    // -------------------------
    // Exact comparison (safety)
    // -------------------------

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
}

public readonly struct StorageItemDescriptor
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