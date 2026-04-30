using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class LaminarStorageFolder : LaminarStorageItem, ILaminarStorageFolder
{
    private readonly ILaminarStorageItemFactory _factory;
    private readonly IFileSystem _fileSystem;
    private readonly IPersistentDataManager _persistentDataManager;
    
    private SourcedObservableCollection<ILaminarStorageItem>? _contentsInternal;
    private bool _persistentContentsDirty = true;
    private bool _isRefreshing;
    
    protected LaminarStorageFolder(
        FileSystemPath fileSystemPath,
        ILaminarStorageItemFactory factory,
        IFileSystem fileSystem,
        IPersistentDictionary persistentData,
        IPersistentDataManager persistentDataManager,
        ILogger<LaminarStorageItem> logger)
        : base(fileSystem, logger, persistentData, fileSystemPath.NameAndExtension)
    {
        _fileSystem = fileSystem;
        _factory = factory;
        _persistentDataManager = persistentDataManager;
        
        if (!fileSystem.Exists(fileSystemPath))
        {
            fileSystem.CreateDirectory(fileSystemPath);
        }
        
        IsExpanded = PersistentStorage[nameof(IsExpanded)].SetDefaultAndGet(false).Value;
    }
    
    public LaminarStorageFolder(
        LaminarStorageFolder parent, 
        ILaminarStorageItemFactory factory, 
        IFileSystem fileSystem, 
        IPersistentDictionary persistentData,
        IPersistentDataManager persistentDataManager,
        ILogger<LaminarStorageItem> logger) 
        : this(parent.Path.ChildPath(persistentData[NameKey].GetValue<string>().Value), 
            factory, fileSystem, persistentData, persistentDataManager, logger)
    {
        SetParent(parent);
        Refresh();
    }
    
    public bool ContentsIsInitialized => _contentsInternal is not null;
    
    public IReadOnlyObservableCollection<ILaminarStorageItem> Contents
    {
        get
        {
            if (_contentsInternal is not null) return _contentsInternal;

            _contentsInternal = new SourcedObservableCollection<ILaminarStorageItem>(PersistentStorage[nameof(Contents)]
                .SetDefaultAndGet(_persistentDataManager.GetHeadlessNode<IPersistentList>()).Value
                .Select(x => _factory.FromPersistentData(x.GetValue<IPersistentDictionary>().Value, this)));
            
            _contentsInternal.CollectionChanged += OnContentsChanged;
            Refresh();
            return _contentsInternal;
        }
    }

    private void OnContentsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isRefreshing) return;
        _persistentContentsDirty = true;
        foreach (var item in e.NewItems?.Cast<LaminarStorageItem>() ?? [])
        {
            item.SetParent(this);
        }
        
        SyncContentsToPersistentData();
    }

    public bool IsExpanded
    {
        get;
        set
        {
            if (!SetField(ref field, value)) return;
            PersistentStorage.SetValue(nameof(IsExpanded), value);
        }
    }
    
    public override void OnEffectivelyEnabledChanged()
    {
        base.OnEffectivelyEnabledChanged();
        if (!ContentsIsInitialized) return; 
        foreach (var storageItem in Contents)
        {
            if (storageItem is not LaminarStorageItem laminarStorageItem) return;
            laminarStorageItem.OnEffectivelyEnabledChanged();
        }
    }

    protected override void RefreshOverride()
    { 
        if (_contentsInternal is null) return;
        _isRefreshing = true;
        _contentsInternal.ChangeSourceTo(GetChildren(), SourcedCollectionMode.SetEquality);
        foreach (var child in Contents)
        {
            child.Refresh();
        }

        _isRefreshing = false;
        SyncContentsToPersistentData();
    }

    private void SyncContentsToPersistentData()
    {
        if (!_persistentContentsDirty || _contentsInternal is null) return;
        
        var newPersistentList = _persistentDataManager.GetHeadlessNode<IPersistentList>();
        foreach (var child in _contentsInternal.Cast<LaminarStorageItem>())
        {
            newPersistentList.AddAndInitialize(child.PersistentStorage);
        }
        
        PersistentStorage[nameof(Contents)].GetValue<IPersistentList>().Value = newPersistentList;

        _persistentContentsDirty = false;
    }
    
    private IEnumerable<ILaminarStorageItem> GetChildren() 
        => _fileSystem.EnumerateChildren(Path).Select(x => _factory.FromPath(x, this));
}