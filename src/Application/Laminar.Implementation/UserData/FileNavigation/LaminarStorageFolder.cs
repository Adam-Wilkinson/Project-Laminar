using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageFolder : LaminarStorageItem<DirectoryInfo>, ILaminarStorageFolder
{
    private readonly ILaminarStorageItemFactory _factory;
    private readonly SourcedObservableCollection<ILaminarStorageItem> _contents;
    private readonly ObservableValue<long> _sizeOnDisk = new();
    private readonly Queue<(ILaminarStorageItem, int)> _queuedMoves = [];
    
    public LaminarStorageFolder(DirectoryInfo directoryInfo, 
        ILaminarStorageItemFactory factory, 
        ILogger<LaminarStorageItem>? logger,
        ILaminarStorageFolder parent) : this(directoryInfo, factory, logger)
    {
        SetParent(this, parent);
    }

    protected LaminarStorageFolder(
        DirectoryInfo directoryInfo, 
        ILaminarStorageItemFactory factory,
        ILogger<LaminarStorageItem>? logger) : base(directoryInfo, logger)
    {
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        _factory = factory;
        _contents = new SourcedObservableCollection<ILaminarStorageItem>(GetChildren())
        {
            SyncMode = SourcedCollectionMode.SetEquality // We have a different item order to the system files
        };
        
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
        base.Refresh();
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
        FileSystemInfo.Refresh();
    }

    private void ChildSizeChanged(object? sender, ObservableValueChangedEventArgs<long> e)
    {
        _sizeOnDisk.Value += e.NewValue - e.OldValue;
    }
    
    private IEnumerable<ILaminarStorageItem> GetChildren()
    {
        IEnumerable<ILaminarStorageItem> returnValue =
            FileSystemInfo.GetFileSystemInfos().Select(x => _factory.FromFileSystemInfo(x, this));

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