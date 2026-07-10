using System.Collections.Immutable;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.Infrastructure;

internal sealed class FileSystemItemHasher(IFileSystem fileSystem) : IFileSystemItemHasher
{
    public bool TryHashItem(IFileSystemItem item, FileSystemPath? pathOverride, out int hash)
    {
        if (StorageItemDescriptor.FromItem(item, pathOverride) is { } descriptor)
        {
            hash = ComputeHash(descriptor);
            return true;
        }

        hash = -1;
        return false;
    }

    public int HashFromPath(FileSystemPath path) 
        => ComputeHash(StorageItemDescriptor.FromPath(path, fileSystem));
    
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

    public static StorageItemDescriptor? FromItem(IFileSystemItem item, FileSystemPath? pathOverride)
    {
        FileSystemPath path = pathOverride ?? item.Path;
        
        return item switch
        {
            IFileSystemFile file => new()
            {
                Name = path.NameAndExtension,
                IsFolder = false,
                Size = file.SizeOnDisk,
            },
            IFileSystemFolder { Contents: not null } folder => new()
            {
                Name = path.NameAndExtension,
                IsFolder = true,
                Children = folder.Contents
                    .Select(child => new StorageItemDescriptor
                    {
                        Name = child.Path.NameAndExtension,
                        IsFolder = child is IFileSystemFolder,
                        Size = child is IFileSystemFile file ? file.SizeOnDisk : null,
                    })
                    .ToImmutableSortedSet(Comparer)
            },
            IFileSystemFolder { Contents: null } => null,
            _ => throw new InvalidOperationException("Unknown storage item type")
        };
    }
}