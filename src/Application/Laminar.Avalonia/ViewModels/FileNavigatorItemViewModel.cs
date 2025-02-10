using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;
using Laminar.Implementation.UserData.FileNavigation;
using Laminar.Implementation.UserData.FileNavigation.UserActions;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.ViewModels;

public partial class FileNavigatorItemViewModel : ViewModelBase
{
    private readonly IUserActionManager _actionManager;
    private readonly ILaminarStorageItemFactory _storageFactory;

    [ObservableProperty] private bool _isExpanded;
    
    public FileNavigatorItemViewModel(ILaminarStorageItem coreItem, IUserActionManager actionManager, ILaminarStorageItemFactory storageFactory)
    {
        _actionManager = actionManager;
        _storageFactory = storageFactory;
        CoreItem = coreItem;
        Children = coreItem is ILaminarStorageFolder coreFolder
            ? new MappedObservableCollection<ILaminarStorageItem, FileNavigatorItemViewModel>(coreFolder.Contents,
                item => new FileNavigatorItemViewModel(item, _actionManager, _storageFactory))
            : null;

        CoreItem.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ILaminarStorageItem.Name))
                OnPropertyChanged(nameof(Name));
        };
    }

    public string Name
    {
        get => CoreItem.Name;
        set => _actionManager.ExecuteAction(new RenameStorageItemAction(value, CoreItem));
    }
    
    public ILaminarStorageItem CoreItem { get; }

    public string ItemTypeName => CoreItem switch
    {
        LaminarStorageFolder => "folder",
        LaminarStorageFile => "script",
        _ => "item"
    };

    public IReadOnlyObservableCollection<FileNavigatorItemViewModel>? Children { get; }

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
            throw new ArgumentException("DeserializationContext must be of type FileNavigatorViewModel", nameof(deserializationContext));
        }

        return fileNavigator.NewItem(storageItemFactory.FromPath(serialized));
    }
}