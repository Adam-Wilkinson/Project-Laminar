using System.ComponentModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.ViewModels.Contracts;
using Laminar.Avalonia.ViewModels.Primitives;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Observables.Collections;
using Laminar.Implementation.Storage.FileExplorer;

namespace Laminar.Avalonia.ViewModels;

public partial class FileNavigatorItemViewModel(
    FileSystemItemType itemType, 
    IFileBrowser fileBrowser, 
    FileExplorerLoadingQueue loadingQueue, 
    Func<FileSystemItemType, FileNavigatorItemViewModel> factory) 
    : ViewModelBase, ITreeViewItemViewModel
{
    public static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
    private static readonly NamesEqualComparer NamesEqual = new();
    
    private readonly Lock _stateLock = new();

    public TreeViewInitializationState InitializationState { get; private set; } = TreeViewInitializationState.Uninitialized;

    public FileNavigatorItemViewModel? Parent { get; private set; }
    
    public IOpenFileService? OpenFileService { get; set; }

    public bool CanChangeIsEnabled => CoreItem?.ParentFolder is { IsEffectivelyEnabled: true };

    public bool IsEffectivelyEnabled => CoreItem?.IsEffectivelyEnabled ?? false;
    
    public bool HasCoreItem => CoreItem is not null;

    public bool IsFolder => CoreItem is IFileSystemFolder;
    
    public bool CanExecuteOpenCommand => CoreItem is IFileSystemFile && !IsOpen;
    
    public FileSystemItemType Type => itemType;
    
    [ObservableProperty] 
    public partial bool NameBeingSet { get; set; } = true;

    [ObservableProperty]
    public partial IObservableList<FileNavigatorItemViewModel>? Children { get; private set; }

    [ObservableProperty] 
    public partial IRuntimeHost? RuntimeHost { get; private set; }

    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(OpenCommand))]
    public partial bool IsOpen { get; private set; }
    
    public bool IsExpanded
    {
        get => (CoreItem as IFileSystemFolder)?.IsExpanded ?? false;
        set
        {
            if (CoreItem is not IFileSystemFolder folder) return;

            if (value)
            {
                EnsureChildrenLoaded();
            }
            
            if (folder.IsExpanded == value)
                return;

            folder.IsExpanded = value;
        }
    }

    public bool IsEnabled
    {
        get => CoreItem?.IsEnabled ?? false;
        set => CoreItem?.IsEnabled = value;
    }

    public string Name
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
            if (CoreItem is null)
            {
                Dispatcher.UIThread.InvokeAsync(async () => await InitializeFromName(Name));
                return;
            }

            if (value != CoreItem.UserFriendlyName)
            {
                Dispatcher.UIThread.InvokeAsync(async () => await fileBrowser.Rename(CoreItem, Name));
            }
        }
    } = itemType.DefaultItemName;

    public IFileSystemItem? CoreItem
    {
        get;
        set
        {
            if (field is not null)
                throw new InvalidOperationException("Cannot initialize an item view model that already has core item");

            field = value;
            if (field is null) throw new ArgumentNullException();

            NameBeingSet = false;
            Name = field.UserFriendlyName;
            if (GetOpenFileService() is { } ofs && field is IFileSystemFile fileCoreItem)
            {
                IsOpen = ofs.FileIsOpen(fileCoreItem);
                ofs.OpenFilesChanged += OpenFilesChanged;
                ofs.OpenFilesChanged += OpenFilesChanged;
            }

            field.PropertyChanged += OnCoreItemPropertyChanged;
            
            OnPropertyChanged(nameof(CanChangeIsEnabled));
            OnPropertyChanged(nameof(IsEffectivelyEnabled));
            OnPropertyChanged(nameof(IsEnabled));
            OpenCommand.NotifyCanExecuteChanged();
            AddItemCommand.NotifyCanExecuteChanged();
            
            if (IsExpanded)
            {
                EnsureChildrenLoaded();
            }

            RuntimeHost = field.GetRootFolder().RuntimeHost;
        }
    }

    [RelayCommand(CanExecute = nameof(IsFolder))]
    private void AddItem(FileSystemItemType newItemType)
    {
        IsExpanded = true;
        Children?.Add(factory(newItemType));
    }

    [RelayCommand(CanExecute = nameof(CanExecuteOpenCommand))]
    private async Task Open()
    {
        if (CoreItem is IFileSystemFile coreFile && GetOpenFileService() is { } ofs)
        {
            await ofs.RequestOpenFile(coreFile);
        }
    }
    
    [RelayCommand]
    private void Rename() => NameBeingSet = true;

    [RelayCommand]
    private Task<IUserActionResult> Delete() =>
        CoreItem is null ? Task.FromResult(IUserActionResult.Ineffectual()) : fileBrowser.Delete(CoreItem);

    [RelayCommand(CanExecute = nameof(HasCoreItem))]
    private void OpenInSystemFileBrowser()
    {
        if (CoreItem is not null) fileBrowser.OpenInSystemFileBrowser(CoreItem);
    }

    public void Refresh()
    {
        CoreItem?.Refresh();
        foreach (var child in Children ?? Enumerable.Empty<FileNavigatorItemViewModel>())
        {
            child.Refresh();
        }
    }

    private async Task InitializeFromName(string name)
    {
        if (Parent?.CoreItem is not IFileSystemFolder parentFolder
            || Parent.Children?.IndexOf(this) is not { } indexInParent)
        {
            throw new InvalidOperationException();
        }
        
        var actionResult = await fileBrowser.Add(name, parentFolder, indexInParent, Type);
        if (actionResult is not UserActionSuccess<IFileSystemItem> successfulAction)
            throw new InvalidOperationException();
        
        CoreItem = successfulAction.ReturnValue;
    }
    
    public async Task EnsureChildrenLoadedAsync()
    {
        IFileSystemFolder folder;
        lock (_stateLock)
        {
            if (InitializationState is not TreeViewInitializationState.Uninitialized) return;
            if (CoreItem is not IFileSystemFolder coreFolder)
            {
                InitializationState = TreeViewInitializationState.ChildrenContentsLoaded;
                return;
            }

            folder = coreFolder;
            InitializationState = TreeViewInitializationState.ChildrenLoading;
        }

        var mapped = (await folder.GetOrLoadContentsAsync()).ObservableMap(x =>
        {
            var result = factory(x.Info);
            result.CoreItem = x;
            return result;
        });

        var children = new SourcedObservableList<FileNavigatorItemViewModel>(mapped, NamesEqual);
        RegisterSubscription(children.SubscribeForEach(child => child.Parent = this));
        
        await Dispatcher.UIThread.InvokeAsync(() => Children = children, DispatcherPriority.ContextIdle);

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
            await child.EnsureChildrenLoadedAsync();
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
    
    private void EnsureChildrenLoaded()
    {
        loadingQueue.Queue(this);
    }
    
    private IOpenFileService? GetOpenFileService()
    {
        FileNavigatorItemViewModel? currentTarget = this;
        var ofs = OpenFileService;
        while (ofs is null && currentTarget is not null)
        {
            ofs = currentTarget.OpenFileService;
            currentTarget = currentTarget.Parent;
        }

        return ofs;
    }

    private void OnCoreItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IFileSystemItem.Path) when CoreItem is not null:
                Name = CoreItem.UserFriendlyName;
                break;
            case nameof(IFileSystemFolder.IsExpanded):
                OnPropertyChanged(nameof(IsExpanded));
                break;
            case nameof(IFileSystemItem.IsEnabled):
                OnPropertyChanged(nameof(IsEnabled));
                break;
            case nameof(IFileSystemItem.IsEffectivelyEnabled):
                OnPropertyChanged(nameof(IsEffectivelyEnabled));
                OnPropertyChanged(nameof(CanChangeIsEnabled));
                UpdateChildrenCanChangeIsEnabled();
                break;
        }
    }

    private void UpdateChildrenCanChangeIsEnabled()
    {
        if (Children is null) return;
        
        foreach (var child in Children)
        {
            child.OnPropertyChanged(nameof(CanChangeIsEnabled));
        }
    }
    
    private void OpenFilesChanged(object? sender, EventArgs _) 
        => IsOpen = CoreItem is IFileSystemFile coreFile && (GetOpenFileService()?.FileIsOpen(coreFile) ?? false);

    private class NamesEqualComparer : IEqualityComparer<FileNavigatorItemViewModel>
    {
        public bool Equals(FileNavigatorItemViewModel? x, FileNavigatorItemViewModel? y) 
            => Equals(x?.Name, y?.Name);

        public int GetHashCode(FileNavigatorItemViewModel obj) => obj.Name.GetHashCode();
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