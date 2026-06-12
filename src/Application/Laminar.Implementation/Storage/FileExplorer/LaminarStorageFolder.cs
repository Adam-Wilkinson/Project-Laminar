using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.PersistentData;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal class LaminarStorageFolder : LaminarStorageItem, ILaminarStorageFolder
{
    private readonly ILaminarStorageItemFactory _factory;
    private readonly IFileSystem _fileSystem;
    
    private SourcedObservableCollection<ILaminarStorageItem>? _contentsInternal;
    private IDisposable? _contentsChangedSubscription;
    
    protected LaminarStorageFolder(
        FileSystemPath fileSystemPath,
        ILaminarStorageItemFactory factory,
        IFileSystem fileSystem,
        IPersistentDictionary persistentData,
        ILogger<LaminarStorageItem> logger)
        : base(fileSystem, logger, persistentData, fileSystemPath.NameAndExtension)
    {
        _fileSystem = fileSystem;
        _factory = factory;
        
        if (!fileSystem.Exists(fileSystemPath))
        {
            fileSystem.CreateDirectory(fileSystemPath);
        }
        
        IsExpanded = PersistentStorage[nameof(IsExpanded)].GetValueOrDefault(false).Value;
    }
    
    public LaminarStorageFolder(
        LaminarStorageFolder parent, 
        ILaminarStorageItemFactory factory, 
        IFileSystem fileSystem, 
        IPersistentDictionary persistentData,
        ILogger<LaminarStorageItem> logger) 
        : this(parent.Path.ChildPath(persistentData[NameKey].GetValue<string>().Value), 
            factory, fileSystem, persistentData, logger)
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

            _contentsInternal = new SourcedObservableCollection<ILaminarStorageItem>([]);

            var persistenceSubscription = PersistentStorage[nameof(Contents)]
                .GetOrCreateCollection<IPersistentList>()
                .InitializeAndSyncTo(_contentsInternal,
                    new EncodableDataAdapter<ILaminarStorageItem, IPersistentDictionary>(
                        item => ((LaminarStorageItem)item).PersistentStorage,
                        storage => _factory.FromPersistentData(storage, this)
                    ));

            var forEachSubscription =
                _contentsInternal.SubscribeForEach(item => ((LaminarStorageItem)item).SetParent(this));
            
            _contentsChangedSubscription = new CompositeDisposable(persistenceSubscription, forEachSubscription);
            Refresh();
            return _contentsInternal;
        }
    }

    public bool IsExpanded
    {
        get;
        set
        {
            if (!SetField(ref field, value)) return;
            PersistentStorage[nameof(IsExpanded)].GetValue<bool>().Value = value;
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
        _contentsInternal.ChangeSourceTo(GetChildren(), SourcedCollectionMode.SetEquality);
        foreach (var child in Contents)
        {
            child.Refresh();
        }
    }

    protected override void OnParentRootFolderDisposed(object? sender, EventArgs e)
    {
        _contentsChangedSubscription?.Dispose();
        base.OnParentRootFolderDisposed(sender, e);
    }

    private IEnumerable<ILaminarStorageItem> GetChildren()
        => _fileSystem.EnumerateChildren(Path).Select(childPath =>
            _factory.CreateChild(childPath.NameAndExtension, this, _fileSystem.IsDirectory(childPath)));
}