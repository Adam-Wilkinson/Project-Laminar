using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.Graph;

internal sealed class FileSystemItemRepository : IMutableFileSystemItemRepository
{
    private readonly Dictionary<FileSystemPath, IFileSystemItem> _items = [];
    private readonly Dictionary<FileSystemPath, IFileSystemItem?> _outdatedItems = [];
    
    public bool TryGetOutdated(FileSystemPath outdatedPath, [NotNullWhen(true)] out IFileSystemItem? item)
    {
        if (_outdatedItems.TryGetValue(outdatedPath, out IFileSystemItem? outdatedItem) && outdatedItem != null)
        {
            item = outdatedItem;
            return true;
        }
        
        item = null;
        return false;
    }

    public void ClearOutdated() => _outdatedItems.Clear();

    public bool TryGetExisting(FileSystemPath path, [NotNullWhen(true)] out IFileSystemItem? item) 
        => _items.TryGetValue(path, out item);

    public void Add(FileSystemGraph.MutationToken token, IFileSystemItem item)
    {
        _items.Add(item.Path, item);

        if (item is not IFileSystemFolder { Contents: not null } folder) return;

        foreach (var child in folder.Contents)
        {
            Add(token, child);
        }
    }

    public void Remove(FileSystemGraph.MutationToken token, IFileSystemItem item)
    {
        if (!_items.Remove(item.Path)) return;

        if (!_outdatedItems.TryAdd(item.Path, item))
        {
            // Several outdated items with the same path: Nullify to signify clash
            _outdatedItems[item.Path] = null;   
        }

        if (item is not IFileSystemFolder { Contents: not null } folder) return;

        foreach (var child in folder.Contents)
        {
            Remove(token, child);
        }
    }
}