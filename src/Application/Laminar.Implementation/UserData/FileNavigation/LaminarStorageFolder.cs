using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageFolder : LaminarStorageItem, ILaminarStorageFolder
{
    private readonly ILaminarStorageItemFactory _factory;
    private readonly SourcedObservableCollection<ILaminarStorageItem> _contents = new([])
    {
        SyncMode = SourcedCollectionMode.SetEquality,
    };
    
    private readonly ObservableValue<long> _sizeOnDisk = new();
    private readonly Queue<(ILaminarStorageItem, int)> _queuedMoves = [];
    private readonly IFileSystem _fileSystem;
    private readonly Lock _getChildrenLock = new();
    
    public LaminarStorageFolder(string path, 
        ILaminarStorageItemFactory factory, 
        ILogger<LaminarStorageItem>? logger,
        IFileSystem fileSystem,
        ILaminarStorageFolder parent) : this(path, factory, fileSystem, logger)
    {
        SetParent(this, parent);
        Refresh();
    }

    protected LaminarStorageFolder(
        string path, 
        ILaminarStorageItemFactory factory,
        IFileSystem fileSystem,
        ILogger<LaminarStorageItem>? logger) : base(logger)
    {
        if (!fileSystem.Exists(path))
        {
            fileSystem.CreateDirectory(path);
        }

        if (System.IO.Path.GetFileName(path) is { } dirName)
        {
            Name = dirName;
        }

        _fileSystem = fileSystem;
        _factory = factory;
        
        Contents.HelperInstance().ItemAdded += ContentsItemAdded;
        Contents.HelperInstance().ItemRemoved += ContentsItemRemoved;
    }
    
    public override IObservableValue<long> SizeOnDisk => _sizeOnDisk;

    public IReadOnlyObservableCollection<ILaminarStorageItem> Contents => _contents;
    
    public void RegisterQueuedMove(ILaminarStorageItem item, int newIndex)
    {
        _queuedMoves.Enqueue((item, newIndex));
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

    public override void Refresh()
    {
        _contents.ChangeSourceTo(GetChildren());
        foreach (var child in Contents)
        {
            child.Refresh();
        }
    }
    
    private void ContentsItemRemoved(object? sender, ItemRemovedEventArgs<ILaminarStorageItem> e)
    {
        e.Item.SizeOnDisk.ValueChanged -= ChildSizeChanged;
    }
    
    private void ContentsItemAdded(object? sender, ItemAddedEventArgs<ILaminarStorageItem> e)
    {
        if (e.Item is not LaminarStorageItem newStorageItem) return;
        SetParent(newStorageItem, this);
        _sizeOnDisk.Value += newStorageItem.SizeOnDisk.Value;
        newStorageItem.SizeOnDisk.ValueChanged += ChildSizeChanged;
    }

    private void ChildSizeChanged(object? sender, ObservableValueChangedEventArgs<long> e)
    {
        _sizeOnDisk.Value += e.NewValue - e.OldValue;
    }
    
    private IEnumerable<ILaminarStorageItem> GetChildren()
    {
        // The refresh action can cause this to be called from separate threads, e.g. on a move we have a simultaneous
        // add and remove
        lock (_getChildrenLock)
        {
            IEnumerable<ILaminarStorageItem> returnValue =
                _fileSystem.EnumerateFileSystemEntries(Path).Select(x => _factory.FromPath(x, this));

            if (_queuedMoves.Count == 0)
            {
                return returnValue;
            }
        
            var listReturn = returnValue.ToList();
            while (_queuedMoves.Count > 0)
            {
                var (movedItem, newIndex) = _queuedMoves.Dequeue();
                int oldIndex = listReturn.TakeWhile(item => item.Name != movedItem.Name).Count();
                if (oldIndex >= listReturn.Count)
                {
                    Logger?.LogError("Failed to remove item from folder children that should be there");
                }
                else
                {
                    var item = listReturn[oldIndex];
                    listReturn.RemoveAt(oldIndex);
                    listReturn.Insert(newIndex, item);
                }
            }   
            return listReturn;
        }
    }
}