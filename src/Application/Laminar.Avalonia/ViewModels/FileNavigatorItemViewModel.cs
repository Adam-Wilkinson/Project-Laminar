using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Laminar.Avalonia.DragDrop;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;
using Laminar.Implementation.UserData.FileNavigation;
using Laminar.Implementation.UserData.FileNavigation.UserActions;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia.ViewModels;

public partial class FileNavigatorItemViewModel : ViewModelBase
{
    private readonly IUserActionManager _actionManager;
    private readonly ILaminarStorageItemFactory _storageFactory;
    private readonly ILogger<FileNavigatorItemViewModel>? _logger;
    private readonly Func<ILaminarStorageItem, FileNavigatorItemViewModel> _factory;
    
    
    [ObservableProperty] private bool _isExpanded = true;
    
    public FileNavigatorItemViewModel(
        ILaminarStorageItem coreItem, 
        IUserActionManager actionManager, 
        ILaminarStorageItemFactory storageFactory,
        IDialogService dialogService, 
        ILogger<FileNavigatorItemViewModel>? logger = null,
        TopLevel? topLevel = null)
    {
        _actionManager = actionManager;
        _storageFactory = storageFactory;
        _logger = logger;
        CoreItem = coreItem;
        Name = CoreItem.Name;
        _factory = storageItem =>
            new FileNavigatorItemViewModel(storageItem, _actionManager, _storageFactory, dialogService, logger,
                topLevel)
            {
                Parent = this,
            };

        if (coreItem is ILaminarStorageFolder folder)
        {
            Children = new ObservableCollection<FileNavigatorItemViewModel>(folder.Contents.Select(_factory));
            folder.Contents.HelperInstance().ItemAdded += (_, e) 
                => Children.Insert(e.Index, _factory(e.Item));
            folder.Contents.HelperInstance().ItemRemoved += (_, e)
                => Children.RemoveAt(e.Index);
            Children.HelperInstance().ItemAdded += (_, e) =>
            {
                e.Item.Parent = this;
            };
        }

        CoreItem.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ILaminarStorageItem.Name)) Name = CoreItem.Name;
        };

        CoreItem.ExceptionRaised += async (_, e) =>
        {
            if (topLevel is null) return;
            await dialogService.ShowError((INotifyPropertyChanged)topLevel.DataContext!, "File System Error",
                e.Message);
        };
    }

    public FileNavigatorItemViewModel? Parent { get; private set; }

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

            if (value != CoreItem.Name && !_actionManager.ExecuteAction(new RenameStorageItemAction(value, CoreItem)))
            {
                field = CoreItem.Name;
                OnPropertyChanged();
            }
        }
    }

    public ILaminarStorageItem CoreItem { get; }

    public string ItemTypeName => CoreItem switch
    {
        LaminarStorageFolder => "folder",
        LaminarStorageFile => "script",
        _ => "item"
    };

    public ObservableCollection<FileNavigatorItemViewModel>? Children { get; }

    [RelayCommand(CanExecute = nameof(IsFolder))]
    public void AddItem(Type itemType)
    {
        if (CoreItem is not ILaminarStorageFolder folder) return;
        if (itemType.IsAssignableTo(typeof(ILaminarStorageFolder)))
        {
            IsExpanded = true;
            _actionManager.ExecuteAction(new AddDefaultStorageItemAction<ILaminarStorageFolder>(folder, _storageFactory));
        }
        else if (itemType.IsAssignableTo(typeof(LaminarStorageFile)))
        {
            IsExpanded = true;
            _actionManager.ExecuteAction(new AddDefaultStorageItemAction<LaminarStorageFile>(folder, _storageFactory));
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
        _actionManager.ExecuteAction(new DeleteStorageItemAction<ILaminarStorageItem>(CoreItem));
    }   
}

public class FileNavigatorItemViewModelSerializer(ILaminarStorageItemFactory storageItemFactory) : TypeSerializer<FileNavigatorItemViewModel, string>
{
    protected override string SerializeTyped(FileNavigatorItemViewModel toSerialize)
        => toSerialize.CoreItem.Path;

    protected override FileNavigatorItemViewModel DeSerializeTyped(string serialized, object? deserializationContext = null)
    {
        if (deserializationContext is not FileNavigatorViewModel fileNavigator)
        {
            throw new ArgumentException(@"DeserializationContext must be of type FileNavigatorViewModel", nameof(deserializationContext));
        }

        return fileNavigator.NewItem(storageItemFactory.FromPath(serialized));
    }
}