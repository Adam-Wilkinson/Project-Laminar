using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.Graph;

internal sealed class FileSystemRoots(
    FileSystemGraph.MutationToken token,
    IMutableFileSystemItemRepository repository,
    IFileSystemItemFactory itemFactory)
    : IFileSystemRoots
{
    private readonly ObservableCollection<IFileSystemRootFolder> _internal = [];

    public IEnumerator<IFileSystemRootFolder> GetEnumerator() => _internal.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _internal.Count;

    public bool TryGetRootFolder(FileSystemPath path, [MaybeNullWhen(false)] out IFileSystemRootFolder folder)
    {
        if (_internal.FirstOrDefault(x => x.Path == path) is { } match)
        {
            folder = match;
            return true;
        }

        folder = null;
        return false;
    }

    public IFileSystemRootFolder CreateRootFolder(FileSystemPath path, IPersistentDictionary data)
    {
        if (_internal.Any(x => x.Path == path))
        {
            throw new InvalidOperationException("Root folder already registered");
        }
        
        var newRoot = CreateDetachedRoot(path, data);
        _internal.Add(newRoot);
        ItemAdded?.Invoke(this, newRoot);
        return newRoot;
    }

    public bool RemoveRootAt(FileSystemPath path, bool removeInfoFiles)
        => RemoveRootAt(path, removeInfoFiles, out _);

    public bool RemoveRootAt(FileSystemPath path, bool removeInfoFiles, out IFileSystemRootFolder? oldRoot)
    {
        if (!repository.TryGetItem(path, out var item))
        {
            oldRoot = null;
            return false;
        }

        if (item is not IFileSystemRootFolder folder)
        {
            throw new InvalidOperationException($"The item at the specified path {path} is not a root folder");
        }
        
        oldRoot = folder;
        ToMutable(oldRoot).OnRemoved(token, removeInfoFiles);
        repository.Remove(token, folder);
        _internal.Remove(folder);
        ItemRemoved?.Invoke(this, folder);
        return true;
    }

    public IFileSystemRootFolder this[int index] => _internal[index];

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => _internal.CollectionChanged += value;
        remove => _internal.CollectionChanged -= value;
    }

    public event EventHandler<IFileSystemRootFolder>? ItemAdded;
    
    public event EventHandler<IFileSystemRootFolder>? ItemRemoved;
    
    public bool Contains(IFileSystemRootFolder value) => _internal.Contains(value);

    public int IndexOf(IFileSystemRootFolder value) => _internal.IndexOf(value);

    internal IFileSystemRootFolder CreateDetachedRoot(FileSystemPath path, IPersistentDictionary data)
    {
        var newFolder = itemFactory.CreateRootFolder(path, data);
        repository.Add(token, newFolder);
        return newFolder;   
    }
    
    private static IMutableFileSystemRootFolder ToMutable(IFileSystemRootFolder? folder)
    {
        if (folder is not IMutableFileSystemRootFolder mutable)
        {
            throw new InvalidOperationException("The storage graph can only change mutable storage items");
        }

        return mutable;
    }
}