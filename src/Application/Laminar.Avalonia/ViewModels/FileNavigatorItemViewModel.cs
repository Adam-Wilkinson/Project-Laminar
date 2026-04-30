using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Extensions;
using Laminar.Domain.Notification;

namespace Laminar.Avalonia.ViewModels;

public partial class FileNavigatorItemViewModel : ViewModelBase, ITreeViewItemViewModel
{
    public static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
    private static readonly ContentsEqualComparer ContentsEqual = new();
    
    private readonly ILaminarFileBrowser _fileBrowser;
    private readonly SourcedObservableCollection<FileNavigatorItemViewModel>? _children;
    private readonly Func<ILaminarStorageItem, FileNavigatorItemViewModel> _fromCoreItemFactory;
    private readonly Func<StorageItemType, FileNavigatorItemViewModel> _fromItemTypeFactory;
    
    private bool _folderContentsInitialized;
    private string _name;

    public FileNavigatorItemViewModel(
        StorageItemType itemType,
        ILaminarFileBrowser fileBrowser,
        Func<StorageItemType, FileNavigatorItemViewModel> factory)
    {
        Type = itemType;
        NameBeingSet = true;
        _fileBrowser = fileBrowser;
        _fromItemTypeFactory = type =>
        {
            var result = factory(type);
            result.Parent = this;
            return result;
        };
        
        _fromCoreItemFactory = coreItem =>
        {
            var result = _fromItemTypeFactory(TypeOf(coreItem));
            result.CoreItem = coreItem;
            return result;
        };
        
        _name = itemType switch
        {
            StorageItemType.Folder => "Untitled Folder",
            StorageItemType.Script => "Untitled Script",
            _ => throw new InvalidOperationException()
        };
        
        if (itemType is StorageItemType.Folder)
        {
            _children = new SourcedObservableCollection<FileNavigatorItemViewModel>([], ContentsEqual);
            _children.HelperInstance().ItemAdded += (_, e) => e.Item.Parent = this;
        }
    }
    
    public FileNavigatorItemViewModel(
        ILaminarStorageItem coreItem, 
        ILaminarFileBrowser fileBrowser, 
        Func<StorageItemType, FileNavigatorItemViewModel> factory) : this(TypeOf(coreItem), fileBrowser, factory)
    {
        CoreItem = coreItem;
        NameBeingSet = false;
    }

    public bool IsExpanded
    {
        get => (CoreItem as ILaminarStorageFolder)?.IsExpanded ?? false; 
        set => (CoreItem as ILaminarStorageFolder)?.IsExpanded = value;
    }

    public FileNavigatorItemViewModel? Parent { get; private set; }

    [ObservableProperty]
    public partial bool NameBeingSet { get; set; }
    
    public bool CanChangeIsEnabled
    {
        get
        {
            if (CoreItem is null) return false;
            if (CoreItem.ParentFolder is null) return true;
            return CoreItem.ParentFolder.IsEffectivelyEnabled;
        }
    }

    public IObservableCollection<FileNavigatorItemViewModel>? Children
    {
        get
        {
            if (_children is null || CoreItem is not ILaminarStorageFolder folder) return null;
            if (_folderContentsInitialized) return _children;

            _children.ChangeSourceTo(folder.Contents.ObservableMap(_fromCoreItemFactory));
            _folderContentsInitialized = true;
            return _children;
        }
    }
    
    public string Name
    {
        get => _name;
        set
        {
            if (value == _name) return;
            _name = value;
            OnPropertyChanged();
            if (CoreItem is null)
            {
                Dispatcher.UIThread.InvokeAsync(async () => await InitializeFromName(Name));
                return;
            }
            
            if (value != CoreItem.UserFriendlyName)
            {
                Dispatcher.UIThread.InvokeAsync(async () => await _fileBrowser.Rename(CoreItem, Name));
            }
        }
    }
    
    public ILaminarStorageItem? CoreItem
    {
        get;
        private set
        {
            if (field is not null)
                throw new InvalidOperationException("Cannot initialize an item view model that already has core item");

            field = value;
            if (field is null) 
                throw new ArgumentNullException();
            
            Name = field.UserFriendlyName;
        
            field.FilterPropertyChanged(nameof(ILaminarStorageItem.Path)).OnNotification += 
                (_, _) => Name = field.UserFriendlyName;

            field.FilterPropertyChanged(nameof(ILaminarStorageFolder.IsExpanded)).OnNotification += 
                (_, _) => OnPropertyChanged(nameof(IsExpanded));

            field.GetDependentValue(x => x.ParentFolder?.IsEffectivelyEnabled ?? false).OnChanged +=
                (_, _) => OnPropertyChanged(nameof(CanChangeIsEnabled));
        }
    }

    public bool HasCoreItem => CoreItem is not null;

    public StorageItemType Type { get; }

    [RelayCommand(CanExecute = nameof(IsFolder))]
    private void AddItem(StorageItemType itemType)
    {
        IsExpanded = true;
        Children?.Add(_fromItemTypeFactory(itemType));
    }

    public bool IsFolder() => CoreItem is ILaminarStorageFolder;

    [RelayCommand]
    private void Rename() => NameBeingSet = true;

    [RelayCommand]
    private Task<IUserActionResult> Delete() =>
        CoreItem is null ? Task.FromResult(IUserActionResult.Invalid()) : _fileBrowser.Delete(CoreItem);

    [RelayCommand(CanExecute = nameof(HasCoreItem))]
    private void OpenInSystemFileBrowser()
    {
        if (CoreItem is not null) _fileBrowser.OpenInSystemFileBrowser(CoreItem);
    }

    public void Refresh()
    {
        CoreItem?.Refresh();
        _children?.SyncFromSource();
        foreach (var child in _children ?? Enumerable.Empty<FileNavigatorItemViewModel>())
        {
            child.Refresh();
        }
    }

    private async Task InitializeFromName(string name)
    {
        if (Parent?.CoreItem is not ILaminarStorageFolder parentFolder) 
            throw new InvalidOperationException();
        var actionResult = await _fileBrowser.Add(name, parentFolder, Type);
        if (actionResult is not UserActionSuccess<ILaminarStorageItem> successfulAction)
            throw new InvalidOperationException();
        
        CoreItem = successfulAction.ReturnValue;
    } 
    
    private static StorageItemType TypeOf(ILaminarStorageItem item) => item switch
    {
        ILaminarStorageFolder => StorageItemType.Folder,
        ILaminarStorageFile => StorageItemType.Script,
        _ => throw new InvalidOperationException()
    };

    private class ContentsEqualComparer : IEqualityComparer<FileNavigatorItemViewModel>
    {
        public bool Equals(FileNavigatorItemViewModel? x, FileNavigatorItemViewModel? y) => Equals(x?.CoreItem?.Path, y?.CoreItem?.Path);

        public int GetHashCode(FileNavigatorItemViewModel obj) => obj.CoreItem?.Path.GetHashCode() ?? -1;
    }
}