using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
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
    private static readonly TimeSpan ExpandHoveredOverFolderDelay = new(0, 0, 0, 0, 500);

    private FileNavigatorItemViewModel? _currentHoveredItem;
    
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
                ReceptacleParent: { } targetParentTreeViewItem,
                ReceptacleParentDataContext: FileNavigatorItemViewModel targetFileNavigatorViewModel,
                ReceptacleIndex: var targetIndex
            })
        {
            return;
        }
        if (eventArgs.DraggingControl.DataContext is not FileNavigatorItemViewModel draggedItem) return;
        
        eventArgs.Handled = true;

        _currentHoveredItem = targetFileNavigatorViewModel;
        
        if (targetFileNavigatorViewModel.Children is null) return;
        var indexInParent = draggedItem.Parent?.Children?.IndexOf(draggedItem); 
        
        // The parent does not contain its child, or this hover does not constitute a move
        if (indexInParent == -1 || (indexInParent == targetIndex && Equals(draggedItem.Parent, targetFileNavigatorViewModel))) return;
        
        // Trying to move a folder into itself
        if (Equals(draggedItem, targetFileNavigatorViewModel)) return;

        // A list with 5 elements cannot have one of its own elements moved to index 5
        if (Equals(draggedItem.Parent, targetFileNavigatorViewModel) && targetIndex == draggedItem.Parent?.Children?.Count) return;
        
        // A closed folder cannot take children, but should be expanded after a certain amount of time
        if (!targetParentTreeViewItem.IsExpanded)
        {
            _ = Task.Delay(ExpandHoveredOverFolderDelay).ContinueWith(_ =>
            {
                if (!Equals(_currentHoveredItem, targetFileNavigatorViewModel)) return;
                Dispatcher.UIThread.Post(() =>
                {
                    targetParentTreeViewItem.IsExpanded = true;
                    // OnHover(eventArgs); 
                });
            });
            return;
        }
        
        draggedItem.Parent?.Children?.Remove(draggedItem);
        targetFileNavigatorViewModel.Children?.Insert(targetIndex, draggedItem);   
    }

    [RelayCommand]
    public void OnDrop(DropTargetEventArgs eventArgs)
    {
        eventArgs.Handled = true;
    }
}