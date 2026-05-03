using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.Shapes;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Extensions;
using Laminar.Domain.Notification;

namespace Laminar.Avalonia.ViewModels;

public partial class FileNavigatorItemViewModel : ViewModelBase, ITreeViewItemViewModel
{
    public static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
    private static readonly NamesEqualComparer NamesEqual = new();
    
    private readonly ILaminarFileBrowser _fileBrowser;
    private readonly SourcedObservableCollection<FileNavigatorItemViewModel>? _children;
    private readonly Func<ILaminarStorageItem, FileNavigatorItemViewModel> _fromCoreItemFactory;
    private readonly Func<StorageItemType, FileNavigatorItemViewModel> _fromItemTypeFactory;
    private readonly Lock _stateLock = new();
    private readonly FileExplorerLoadingQueue _loadingQueue;


    private string _name;

    public FileNavigatorItemViewModel(
        StorageItemType itemType,
        ILaminarFileBrowser fileBrowser,
        FileExplorerLoadingQueue loadingQueue,
        Func<StorageItemType, FileNavigatorItemViewModel> factory)
    {
        Type = itemType;
        NameBeingSet = true;
        _fileBrowser = fileBrowser;
        _loadingQueue = loadingQueue;
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
        
        if (Type is StorageItemType.Folder)
        {
            _children = new SourcedObservableCollection<FileNavigatorItemViewModel>([], NamesEqual);
            _children.HelperInstance().ItemAdded += (_, e) => e.Item.Parent = this;
        }
    }
    
    public FileNavigatorItemViewModel(
        ILaminarStorageItem coreItem, 
        ILaminarFileBrowser fileBrowser, 
        FileExplorerLoadingQueue loadingQueue,
        Func<StorageItemType, FileNavigatorItemViewModel> factory) : this(TypeOf(coreItem), fileBrowser, loadingQueue, factory)
    {
        CoreItem = coreItem;
    }

    public TreeViewInitializationState InitializationState { get; private set; } = TreeViewInitializationState.Uninitialized;
    
    public bool IsExpanded
    {
        get => (CoreItem as ILaminarStorageFolder)?.IsExpanded ?? false;
        set
        {
            if (CoreItem is not ILaminarStorageFolder folder) return;

            if (value)
            {
                EnsureChildrenLoaded();
            }
            
            if (folder.IsExpanded == value)
                return;

            folder.IsExpanded = value;
        }
    }

    private void EnsureChildrenLoaded()
    {
        _loadingQueue.Queue(this);
    }

    public Geometry? IconGeometry => (Type, IsExpanded) switch
    {
        (StorageItemType.Script, _) => PathData.ScriptIcon,
        (StorageItemType.Folder, false) => PathData.FolderIcon,
        (StorageItemType.Folder, true) => PathData.FolderOpenIcon,
        _ => PathData.ExclamationMark,
    };

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

    public bool IsEnabled
    {
        get => CoreItem?.IsEnabled ?? false;
        set => CoreItem?.IsEnabled = value;
    }

    public bool IsEffectivelyEnabled => CoreItem?.IsEffectivelyEnabled ?? false;

    public IObservableCollection<FileNavigatorItemViewModel>? Children => _children;
    
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
            if (field is null) throw new ArgumentNullException();

            NameBeingSet = false;
            Name = field.UserFriendlyName;
        
            field.FilterPropertyChanged(nameof(ILaminarStorageItem.Path)).OnNotification += 
                (_, _) => Name = field.UserFriendlyName;
            
            field.FilterPropertyChanged(nameof(ILaminarStorageFolder.IsExpanded)).OnNotification +=
                (_, _) =>
                {
                    OnPropertyChanged(nameof(IsExpanded));
                    OnPropertyChanged(nameof(IconGeometry));
                };
            
            field.FilterPropertyChanged(nameof(ILaminarStorageItem.IsEnabled)).OnNotification +=
                (_, _) => OnPropertyChanged(nameof(IsEnabled));
            
            field.FilterPropertyChanged(nameof(ILaminarStorageItem.IsEffectivelyEnabled)).OnNotification +=
                (_, _) => OnPropertyChanged(nameof(IsEffectivelyEnabled));
            
            field.GetDependentValue(x => x.ParentFolder?.IsEffectivelyEnabled ?? false).OnChanged +=
                (_, _) => OnPropertyChanged(nameof(CanChangeIsEnabled));
            
            OnPropertyChanged(nameof(CanChangeIsEnabled));
            OnPropertyChanged(nameof(IsEffectivelyEnabled));
            OnPropertyChanged(nameof(IsEnabled));

            if (IsExpanded)
            {
                EnsureChildrenLoaded();
            }
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
        if (Parent?.CoreItem is not ILaminarStorageFolder parentFolder
            || Parent.Children?.IndexOf(this) is not { } indexInParent)
        {
            throw new InvalidOperationException();
        }
        var actionResult = await _fileBrowser.Add(name, parentFolder, indexInParent, Type);
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

    private class NamesEqualComparer : IEqualityComparer<FileNavigatorItemViewModel>
    {
        public bool Equals(FileNavigatorItemViewModel? x, FileNavigatorItemViewModel? y) 
            => Equals(x?.Name, y?.Name);

        public int GetHashCode(FileNavigatorItemViewModel obj) => obj.Name.GetHashCode();
    }

    public async Task LoadContentsAsync()
    {
        ILaminarStorageFolder folder;
        lock (_stateLock)
        {
            if (InitializationState is not TreeViewInitializationState.Uninitialized) return;
            if (CoreItem is not ILaminarStorageFolder coreFolder)
            {
                InitializationState = TreeViewInitializationState.ChildrenContentsLoaded;
                return;
            }

            folder = coreFolder;
            InitializationState = TreeViewInitializationState.ChildrenLoading;
        }
            
        var mapped = folder.Contents.ObservableMap(_fromCoreItemFactory);
        await Dispatcher.UIThread.InvokeAsync(() => _children?.ChangeSourceTo(mapped), DispatcherPriority.ContextIdle);

        lock (_stateLock)
        {
            InitializationState = TreeViewInitializationState.ChildrenContentsUnloaded;
        }
    }
    
    public async Task LoadChildrenContentsAsync()
    {
        lock (_stateLock)
        {
            if (Children is null)
            {
                InitializationState = TreeViewInitializationState.ChildrenContentsLoaded;
                return;
            }

            if (InitializationState != TreeViewInitializationState.ChildrenContentsUnloaded) return;
            InitializationState = TreeViewInitializationState.ChildrenContentsLoading;
        }
        
        foreach (var child in Children)
        {
            await child.LoadContentsAsync();
        }

        lock (_stateLock)
        {
            InitializationState = TreeViewInitializationState.ChildrenContentsLoaded;
        }
    }

    public void ResetLoadState()
    {
        lock (_stateLock)
        {
            InitializationState = TreeViewInitializationState.Uninitialized;
        }
    }
}
    
public enum TreeViewInitializationState
{
    Uninitialized,
    ChildrenLoading,
    ChildrenContentsUnloaded,
    ChildrenContentsLoading,
    ChildrenContentsLoaded,
}