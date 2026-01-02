using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.UserData.FileNavigation;

public class LaminarStorageFolder : LaminarStorageItem<DirectoryInfo>, ILaminarStorageFolder
{
    private readonly ILaminarStorageItemFactory _factory;
    
    private ILaminarStorageFolder? _parent;
    
    public LaminarStorageFolder(string path, 
        ILaminarStorageItemFactory factory, 
        ILogger<ILaminarStorageItem>? logger,
        ILaminarStorageFolder? parent = null) : this(new DirectoryInfo(path), factory, logger, parent)
    {
    }
    
    public LaminarStorageFolder(DirectoryInfo directoryInfo, 
        ILaminarStorageItemFactory factory, 
        ILogger<ILaminarStorageItem>? logger,
        ILaminarStorageFolder? parent = null) : base(directoryInfo, logger)
    {
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        _factory = factory;
        
        Contents = new ObservableCollection<ILaminarStorageItem>(GetChildren());
        Contents.HelperInstance().ItemAdded += ContentsItemAdded;
        Contents.HelperInstance().ItemRemoved += ContentsItemRemoved;
        _parent = parent;
    }

    private void ContentsItemRemoved(object? sender, ItemRemovedEventArgs<ILaminarStorageItem> e)
    {
        e.Item.Delete();
    }

    private void ContentsItemAdded(object? sender, ItemAddedEventArgs<ILaminarStorageItem> e)
    {
        FileSystemInfo.Refresh();
    }

    public ObservableCollection<ILaminarStorageItem> Contents { get; }
    
    public override ILaminarStorageFolder ParentFolder => _parent ??= new LaminarStorageFolder(FileSystemInfo.Parent!, _factory, Logger);
    
    public override bool IsEnabled
    {
        get => base.IsEnabled;
        set
        {
            base.IsEnabled = value;
            EffectivelyEnabledChanged();
        }
    }

    protected override void TryMoveTo(string newPath)
    {
        FileSystemInfo.MoveTo(newPath);
    }

    private IEnumerable<ILaminarStorageItem> GetChildren() => FileSystemInfo.GetDirectories()
        .Select(x => _factory.FromFileSystemInfo(x, this))
        .Concat(FileSystemInfo.GetFiles().Select(x => _factory.FromFileSystemInfo(x, this)));
    
    private void ParentEnabledChange(bool newValue)
    {
        ParentIsEffectivelyEnabled = newValue;
        EffectivelyEnabledChanged();
    }

    private void EffectivelyEnabledChanged()
    {
        OnPropertyChanged(nameof(IsEffectivelyEnabled));
        foreach (var storageItem in Contents)
        {
            switch (storageItem)
            {
                case LaminarStorageFile file:
                    file.ParentEnabledChanged(IsEffectivelyEnabled);
                    break;
                case LaminarStorageFolder folder:
                    folder.ParentEnabledChange(IsEffectivelyEnabled);
                    break;
            }
        }
    }
}