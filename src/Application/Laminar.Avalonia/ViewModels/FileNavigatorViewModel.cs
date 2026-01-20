using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Laminar.Avalonia.DragDrop;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Implementation.UserData.FileNavigation.UserActions;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia.ViewModels;

public partial class FileNavigatorViewModel(
    IUserActionManager actionManager, 
    IPersistentDataManager dataManager, 
    ILaminarStorageItemFactory storageItemFactory,
    IDialogService dialogService,
    ILogger<FileNavigatorItemViewModel>? logger = null,
    TopLevel? topLevel = null)
    : ViewModelBase
{
    [Serialize]
    public ObservableCollection<FileNavigatorItemViewModel> RootFiles { get; set; } = [ 
        new(storageItemFactory.FromPath(Path.Combine(dataManager.Path, "Default")), actionManager, storageItemFactory, dialogService, logger, topLevel) 
    ];

    public FileNavigatorItemViewModel NewItem(ILaminarStorageItem coreItem) =>
        new(coreItem, actionManager, storageItemFactory, dialogService, logger, topLevel);
    
    public void OpenFilePicker()
    {
    }
    
    [RelayCommand]
    public void OnHover(DropTargetEventArgs eventArgs)
    {
        if (eventArgs.ReceptacleTag is not TreeViewDropAcceptor.TreeViewItemReceptacleInfo
            {
                ReceptacleParentDataContext: FileNavigatorItemViewModel targetFileNavigatorViewModel,
                ReceptacleIndex: var targetIndex
            })
        {
            return;
        }
        if (eventArgs.DraggingControl.DataContext is not FileNavigatorItemViewModel draggedItem) return;
        
        eventArgs.Handled = true;
        
        if (targetFileNavigatorViewModel.Children is null) return;
        var indexInParent = draggedItem.Parent?.Children?.IndexOf(draggedItem); 
        
        // The parent does not contain its child, or this hover does not constitute a move
        if (indexInParent == -1 || (indexInParent == targetIndex && Equals(draggedItem.Parent, targetFileNavigatorViewModel))) return;
        
        // Trying to move a folder into itself
        if (Equals(draggedItem, targetFileNavigatorViewModel)) return;

        // A list with 5 elements cannot have one of its own elements moved to index 5
        if (Equals(draggedItem.Parent, targetFileNavigatorViewModel) && targetIndex == draggedItem.Parent?.Children?.Count) return;
        
        draggedItem.Parent?.Children?.Remove(draggedItem);
        targetFileNavigatorViewModel.Children?.Insert(targetIndex, draggedItem);   
    }

    [RelayCommand]
    public void OnDrop(DropTargetEventArgs eventArgs)
    {
        eventArgs.Handled = true;
    }
}