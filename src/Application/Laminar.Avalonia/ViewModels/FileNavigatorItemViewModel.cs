using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;
using Laminar.Implementation.UserData.FileNavigation;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia.ViewModels;

public partial class FileNavigatorItemViewModel : ViewModelBase, ITreeViewItemViewModel
{
    private static readonly ContentsEqualComparer ContentsEqual = new();
    
    private readonly ILaminarFileBrowser _fileBrowser;
    private readonly ILogger<FileNavigatorItemViewModel>? _logger;
    private readonly Func<ILaminarStorageItem, FileNavigatorItemViewModel> _factory;
    
    
    [ObservableProperty] private bool _isExpanded = true;
    
    public FileNavigatorItemViewModel(
        ILaminarStorageItem coreItem, 
        ILaminarFileBrowser fileBrowser, 
        IDialogService dialogService, 
        ILogger<FileNavigatorItemViewModel>? logger = null,
        TopLevel? topLevel = null)
    {
        _fileBrowser = fileBrowser;
        _logger = logger;
        CoreItem = coreItem;
        Name = CoreItem.Name;
        _factory = storageItem =>
            new FileNavigatorItemViewModel(storageItem, _fileBrowser, dialogService, logger, topLevel)
            {
                Parent = this,
            };

        if (coreItem is ILaminarStorageFolder folder)
        {
            Children = new SourcedObservableCollection<FileNavigatorItemViewModel>(folder.Contents.ObservableMap(_factory), ContentsEqual);
            Children.HelperInstance().ItemAdded += (_, e) => e.Item.Parent = this;
        }

        CoreItem.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ILaminarStorageItem.Name)) Name = CoreItem.Name;
        };

        CoreItem.ExceptionRaised += async (_, e) =>
        {
            if (topLevel is null) return;
            await dialogService.ShowError((INotifyPropertyChanged)topLevel.DataContext!, "File System Error", e.Message);
        };
    }


    public FileNavigatorItemViewModel? Parent { get; private set; }
    
    public IObservableCollection<FileNavigatorItemViewModel>? Children { get; }
    
    public string Name
    {
        get;
        set
        {
            if (value != field)
            {
                field = value;
                OnPropertyChanged();
            }

            if (value != CoreItem.Name && !_fileBrowser.Rename(CoreItem, value))
            {
                field = CoreItem.Name;
                OnPropertyChanged();
            }
        }
    }

    public ILaminarStorageItem CoreItem { get; }

    public string ItemTypeName => CoreItem switch
    {
        ILaminarStorageFolder => "folder",
        LaminarStorageFile => "script",
        _ => "item"
    };

    [RelayCommand(CanExecute = nameof(IsFolder))]
    public void AddItem(Type itemType)
    {
        if (CoreItem is not ILaminarStorageFolder folder) return;
        if (itemType.IsAssignableTo(typeof(ILaminarStorageFolder)))
        {
            IsExpanded = true;
            _fileBrowser.AddDefault<ILaminarStorageFolder>(folder);
        }
        else if (itemType.IsAssignableTo(typeof(LaminarStorageFile)))
        {
            IsExpanded = true;
            _fileBrowser.AddDefault<LaminarStorageFile>(folder);
        }
    }

    public bool IsFolder() => CoreItem is ILaminarStorageFolder;

    [RelayCommand]
    public void Rename()
    {
        CoreItem.NeedsName = true;
    }

    [RelayCommand]
    public void Delete()
    {
        _fileBrowser.Delete(CoreItem);
    }

    private class ContentsEqualComparer : IEqualityComparer<FileNavigatorItemViewModel>
    {
        public bool Equals(FileNavigatorItemViewModel? x, FileNavigatorItemViewModel? y) 
            => Equals(x?.CoreItem, y?.CoreItem);

        public int GetHashCode(FileNavigatorItemViewModel obj) => obj.CoreItem.GetHashCode();
    }
}