using System.Collections.Generic;
using System.IO;
using System.Linq;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageFolder : LaminarStorageItem<DirectoryInfo>, ILaminarStorageFolder
{
    private readonly ILaminarStorageItemFactory _factory;
    private readonly SourcedObservableCollection<ILaminarStorageItem> _contents;

    public LaminarStorageFolder(DirectoryInfo directoryInfo, 
        ILaminarStorageItemFactory factory, 
        IFileSystem fileSystem,
        ILogger<LaminarStorageItem>? logger,
        ILaminarStorageFolder? parent = null) : base(directoryInfo, fileSystem, logger)
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
        ParentFolder = parent;
    }

    private void ContentsItemAdded(object? sender, ItemAddedEventArgs<ILaminarStorageItem> e)
    {
        if (e.Item is not LaminarStorageItem newStorageItem) return;
        newStorageItem.ParentFolder = this;
        FileSystemInfo.Refresh();
    }

    public IReadOnlyObservableCollection<ILaminarStorageItem> Contents => _contents;

    protected override void MoveTo(string newPath)
    {
        FileSystemInfo.MoveTo(newPath);
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
    }
    
    private IEnumerable<ILaminarStorageItem> GetChildren() =>
        FileSystemInfo.GetFileSystemInfos().Select(x => _factory.FromFileSystemInfo(x, this));
}