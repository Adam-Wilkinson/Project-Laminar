using System.Collections.Generic;
using System.Linq;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class LaminarStorageFolder : LaminarStorageItem, ILaminarStorageFolder
{
    private readonly ILaminarStorageItemFactory _factory;
    private readonly IFileSystem _fileSystem;
    private bool _contentsInitialized;

    public SourcedObservableCollection<ILaminarStorageItem> ContentsInternal { get; } = new([])
    {
        SyncMode = SourcedCollectionMode.SetEquality
    };

    public LaminarStorageFolder(FileSystemPath path,
        ILaminarStorageFolder parent, 
        ILaminarStorageItemFactory factory, 
        IFileSystem fileSystem, 
        ILogger<LaminarStorageItem> logger) : this(path, factory, fileSystem, logger)
    {
        SetParent(parent);
        Rename(path.Name);
        Refresh();
    }

    protected LaminarStorageFolder(
        FileSystemPath path, 
        ILaminarStorageItemFactory factory,
        IFileSystem fileSystem,
        ILogger<LaminarStorageItem> logger) : base(fileSystem, logger)
    {
        if (!fileSystem.Exists(path))
        {
            fileSystem.CreateDirectory(path);
        }

        _fileSystem = fileSystem;
        _factory = factory;
        
        ContentsInternal.HelperInstance().ItemAdded += ContentsItemAdded;
    }

    public IReadOnlyObservableCollection<ILaminarStorageItem> Contents
    {
        get
        {
            if (_contentsInitialized) return ContentsInternal;

            _contentsInitialized = true;
            Refresh();
            return ContentsInternal;
        }
    }

    public bool IsExpanded { get; set => SetField(ref field, value); }

    public override void OnEffectivelyEnabledChanged()
    {
        base.OnEffectivelyEnabledChanged();
        foreach (var storageItem in Contents)
        {
            if (storageItem is not LaminarStorageItem laminarStorageItem) return;
            laminarStorageItem.OnEffectivelyEnabledChanged();
        }
    }

    protected override void RefreshOverride()
    { 
        if (!_contentsInitialized) return;
        ContentsInternal.ChangeSourceTo(GetChildren());
        foreach (var child in Contents)
        {
            child.Refresh();
        }
    }
    
    private void ContentsItemAdded(object? sender, ItemAddedEventArgs<ILaminarStorageItem> e)
    {
        if (e.Item is not LaminarStorageItem newStorageItem) return;
        newStorageItem.SetParent(this);
    }

    private IEnumerable<ILaminarStorageItem> GetChildren() 
        => _fileSystem.EnumerateChildren(Path).Select(x => _factory.FromPath(x, this));
}