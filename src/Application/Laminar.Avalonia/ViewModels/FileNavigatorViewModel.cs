using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Avalonia.ViewModels;
public class FileNavigatorViewModel(
    IUserActionManager actionManager, 
    IPersistentDataManager dataManager, 
    ILaminarStorageItemFactory storageItemFactory,
    IDialogService dialogService,
    TopLevel? topLevel = null)
    : ViewModelBase
{
    [Serialize]
    public ObservableCollection<FileNavigatorItemViewModel> RootFiles { get; set; } = [ 
        new(storageItemFactory.FromPath(Path.Combine(dataManager.Path, "Default")), actionManager, storageItemFactory, dialogService, topLevel) 
    ];

    public FileNavigatorItemViewModel NewItem(ILaminarStorageItem coreItem) =>
        new(coreItem, actionManager, storageItemFactory, dialogService, topLevel);
    
    public void OpenFilePicker()
    {
    }
}