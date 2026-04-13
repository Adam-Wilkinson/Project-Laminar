using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Extensions;
using Laminar.Domain.Notification;

namespace Laminar.Avalonia.ViewModels;

public partial class FileNavigatorItemViewModel : ViewModelBase, ITreeViewItemViewModel
{
    public static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
    private static readonly ContentsEqualComparer ContentsEqual = new();
    
    private readonly ILaminarFileBrowser _fileBrowser;
    private readonly SourcedObservableCollection<FileNavigatorItemViewModel>? _children;

    private readonly Func<ILaminarStorageItem, FileNavigatorItemViewModel> _factory;

    private bool _folderContentsInitialized;
    
    public FileNavigatorItemViewModel(
        ILaminarStorageItem coreItem, 
        ILaminarFileBrowser fileBrowser, 
        Func<ILaminarStorageItem, FileNavigatorItemViewModel> factory)
    {
        _fileBrowser = fileBrowser;
        _factory = item =>
        {
            var result = factory(item);
            result.Parent = this;
            return result;
        };
        CoreItem = coreItem;
        Name = CoreItem.Path.Name;

        if (coreItem is ILaminarStorageFolder)
        {
            _children = new SourcedObservableCollection<FileNavigatorItemViewModel>([], ContentsEqual);
            _children.HelperInstance().ItemAdded += (_, e) => e.Item.Parent = this;
        }

        CoreItem.FilterPropertyChanged(nameof(ILaminarStorageItem.Path)).OnNotification += (_, _) =>
        {
            Name = CoreItem.Path.Name;
        };

        CoreItem.GetDependentValue(item => item.ParentFolder?.IsEffectivelyEnabled ?? false).OnChanged +=
            (_, _) => OnPropertyChanged(nameof(CanChangeIsEnabled));

        IsExpanded = CoreItem is ILaminarStorageRootFolder;
    }
    
    [ObservableProperty]
    public partial bool IsExpanded { get; set; }
    
    public FileNavigatorItemViewModel? Parent { get; private set; }

    public bool CanChangeIsEnabled => CoreItem.ParentFolder?.IsEffectivelyEnabled ?? true;

    public IObservableCollection<FileNavigatorItemViewModel>? Children
    {
        get
        {
            if (_children is null || CoreItem is not ILaminarStorageFolder folder) return null;
            if (_folderContentsInitialized) return _children;

            _children.ChangeSourceTo(folder.Contents.ObservableMap(_factory));
            _folderContentsInitialized = true;
            return _children;
        }
    }
    
    public string Name
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
            if (value != CoreItem.Path.Name)
            {
                Dispatcher.UIThread.InvokeAsync(async () => _fileBrowser.Rename(CoreItem, Name));
            }
        }
    }

    public ILaminarStorageItem CoreItem { get; }

    public string ItemTypeName => CoreItem switch
    {
        ILaminarStorageFolder => "folder",
        ILaminarStorageItem => "script",
        _ => "item"
    };
    
    [RelayCommand(CanExecute = nameof(IsFolder))]
    public Task AddItem(Type itemType)
    {
        if (CoreItem is not ILaminarStorageFolder folder) return Task.CompletedTask;
        if (itemType.IsAssignableTo(typeof(ILaminarStorageFolder)))
        {
            IsExpanded = true;
            return _fileBrowser.AddDefault<ILaminarStorageFolder>(folder);
        }
        else if (itemType.IsAssignableTo(typeof(ILaminarStorageItem)))
        {
            IsExpanded = true;
            return _fileBrowser.AddDefault<ILaminarStorageItem>(folder);
        }

        return Task.CompletedTask;
    }

    public bool IsFolder() => CoreItem is ILaminarStorageFolder;

    [RelayCommand]
    public void Rename() => CoreItem.NeedsName = true;

    [RelayCommand]
    public Task Delete() => _fileBrowser.Delete(CoreItem);

    [RelayCommand]
    public void OpenInSystemFileBrowser() => _fileBrowser.OpenInSystemFileBrowser(CoreItem);

    public void Refresh()
    {
        CoreItem.Refresh();
        _children?.SyncFromSource();
        foreach (var child in _children ?? Enumerable.Empty<FileNavigatorItemViewModel>())
        {
            child.Refresh();
        }
    }
    
    private class ContentsEqualComparer : IEqualityComparer<FileNavigatorItemViewModel>
    {
        public bool Equals(FileNavigatorItemViewModel? x, FileNavigatorItemViewModel? y) => Equals(x?.CoreItem.Path, y?.CoreItem.Path);

        public int GetHashCode(FileNavigatorItemViewModel obj) => obj.CoreItem.Path.GetHashCode();
    }
}