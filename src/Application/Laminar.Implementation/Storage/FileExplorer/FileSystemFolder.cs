using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Collections;
using Laminar.Implementation.Storage.FileExplorer.Graph;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class FileSystemFolder : FileSystemItem, IMutableFileSystemFolder
{
    private readonly IFileSystemGraph _graph;
    private readonly IFileSystem _fileSystem;
    private readonly Lock _loadContentsLock = new();
    
    private IObservableCollection<IFileSystemItem>? _contentsInternal;
    private Task<IReadOnlyObservableCollection<IFileSystemItem>>? _loadContentsTask;
    private IPersistentList? _persistentContents;
    
    protected FileSystemFolder(
        IPersistentDictionary persistentData,
        IFileSystem fileSystem,
        IFileSystemGraph graph)
        : base(persistentData, fileSystem, graph)
    {
        _fileSystem = fileSystem;
        _graph = graph;
        IsExpanded = PersistentStorage[nameof(IsExpanded)].GetValueOrInitialize(false).Value;
    }
    
    public FileSystemFolder(
        IFileSystemFolder parent, 
        IPersistentDictionary persistentData,
        IFileSystem fileSystem,
        IFileSystemGraph graph) 
        : this(persistentData, fileSystem, graph)
    {
        SetParent(parent);
        Refresh();
    }
    
    public IReadOnlyObservableCollection<IFileSystemItem>? Contents => _contentsInternal;

    public bool IsExpanded
    {
        get;
        set
        {
            if (!SetField(ref field, value)) return;
            PersistentStorage[nameof(IsExpanded)].GetValue<bool>().Value = value;
        }
    }
    
    public override FileSystemItemType Info => FileSystemItemType.Folder;
    
    internal override void OnEffectivelyEnabledChanged()
    {
        base.OnEffectivelyEnabledChanged();
        if (Contents is null) return; 
        foreach (var storageItem in Contents)
        {
            if (storageItem is not FileSystemItem laminarStorageItem) return;
            laminarStorageItem.OnEffectivelyEnabledChanged();
        }
    }

    protected override void RefreshOverride()
    {
        if (Contents is null) return;

        foreach (var childPath in _fileSystem.EnumerateChildren(Path))
        {
            if (Contents.Any(child => child.Path == childPath)) return;

            if (_fileSystem.IsDirectory(childPath))
            {
                _graph.AddFolder(this, 0, childPath.NameAndExtension);
            }
            else
            {
                _graph.AddFile(this, 0, childPath.NameAndExtension);
            }
        }
        
        foreach (var child in Contents)
        {
            child.Refresh();
        }
    }

    protected override void OnDeletedOverride()
    {
        if (Contents is null) return;
        foreach (var storageItem in Contents)
        {
            (storageItem as IMutableFileSystemItem)?.OnDeleted();
        }
    }

    public void InsertChildInternal(FileSystemGraph.MutationToken _, IFileSystemItem newChild, int index)
    {
        if (_loadContentsTask is null)
        {
            GetOrLoadContents();
        }
        
        _contentsInternal?.Insert(index, newChild);
        if (newChild is not FileSystemItem childInternal) throw new InvalidOperationException();
        _persistentContents?.Insert(index).GetOrCreateCollection(childInternal.PersistentStorage);
    }

    public void RemoveChildInternal(FileSystemGraph.MutationToken _, IFileSystemItem child)
    {
        if (_loadContentsTask is null)
        {
            GetOrLoadContents();
        }
        
        var childIndex = _contentsInternal?.IndexOf(child);
        if (childIndex is null or -1) return;
        _contentsInternal?.RemoveAt(childIndex.Value);
        _persistentContents?.RemoveAt(childIndex.Value);
    }

    public void MoveChildInternal(FileSystemGraph.MutationToken _, int oldIndex, int newIndex)
    {
        if (_loadContentsTask is null)
        {
            GetOrLoadContents();
        }
        
        _contentsInternal?.Move(oldIndex, newIndex);
        _persistentContents?.Move(oldIndex, newIndex, 1);
    }

    public Task<IReadOnlyObservableCollection<IFileSystemItem>> GetOrLoadContentsAsync()
    {
        lock (_loadContentsLock)
        {
            return _loadContentsTask ??= Task.Run(LoadContents);
        }
    }
    
    public IReadOnlyObservableCollection<IFileSystemItem> GetOrLoadContents()
    {
        if (_contentsInternal is not null) return _contentsInternal;
        
        return GetOrLoadContentsAsync().GetAwaiter().GetResult();
    }

    private IReadOnlyObservableCollection<IFileSystemItem> LoadContents()
    {
        _contentsInternal = new ObservableCollectionImpl<IFileSystemItem>([]);
        
        // When loading persistent contents from memory, we don't want changes to propagate back to _persistentContents
        _persistentContents = null;
        var persistentContents = PersistentStorage[nameof(Contents)].GetOrCreateCollection<IPersistentList>();
        foreach (var persistentDictionary in persistentContents
                     .Select(x => x.GetOrCreateCollection<IPersistentDictionary>()))
        {
            _graph.AddFromPersistentData(this, persistentDictionary);
        }

        _persistentContents = persistentContents;
        
        Refresh();
        
        return _contentsInternal;
    }
}