using System.Collections.Generic;
using System.Linq;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

internal class LaminarStorageFolder : LaminarStorageItem, ILaminarStorageFolder
{
    private readonly ILaminarStorageItemFactory _factory;
    private bool _contentsInitialized;
    private readonly SourcedObservableCollection<ILaminarStorageItem> _contents = new([])
    {
        SyncMode = SourcedCollectionMode.SetEquality
    };
    
    private readonly ObservableValue<long> _sizeOnDisk = new(0);
    private readonly IFileSystem _fileSystem;
    
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
        
        _contents.HelperInstance().ItemAdded += ContentsItemAdded;
    }
    
    public IReadOnlyObservableCollection<ILaminarStorageItem> Contents
    {
        get
        {
            if (_contentsInitialized) return _contents;

            _contentsInitialized = true;
            Refresh();
            return _contents;
        }
    }
    
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
        _contents.ChangeSourceTo(GetChildren());
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