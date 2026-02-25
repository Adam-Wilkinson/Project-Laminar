using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Laminar.Avalonia.DragDrop;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia.ViewModels;

public partial class FileNavigatorViewModel(
    ILaminarFileBrowser fileBrowser,
    IDialogService dialogService,
    ILogger<FileNavigatorItemViewModel>? logger = null,
    TopLevel? topLevel = null)
    : ViewModelBase
{
    private static readonly TimeSpan ExpandHoveredOverFolderDelay = new(0, 0, 0, 0, 500);

    private FileNavigatorItemViewModel? _proposedHoveredItem;
    private (FileNavigatorItemViewModel moveItem, int moveIndex)? _currentHoverMove;

    public IReadOnlyObservableCollection<FileNavigatorItemViewModel> RootFiles { get; set; } = 
        fileBrowser.RootFolders.ObservableMap(rootFolder => 
            new FileNavigatorItemViewModel(rootFolder, fileBrowser, dialogService, logger, topLevel));

    public void OpenFilePicker()
    {
    }
    
    [RelayCommand]
    public void OnHover(DropTargetEventArgs eventArgs)
    {
        if (GetMoveFromDragInfo(eventArgs) is not var (draggedItem, targetParent, targetIndex))
        {
            return;
        }
        
        eventArgs.Handled = true;
        
        _proposedHoveredItem = targetParent;
        
        if (!IsValidMove(draggedItem, targetParent, targetIndex)) return;
        
        // A closed folder cannot take children, but should be expanded after a certain amount of time
        if (!targetParent.IsExpanded)
        {
            _ = Task.Delay(ExpandHoveredOverFolderDelay).ContinueWith(_ =>
            {
                if (!Equals(_proposedHoveredItem, targetParent)) return;
                Dispatcher.UIThread.Post(() =>
                {
                    targetParent.IsExpanded = true;
                    OnHover(eventArgs); 
                });
            });
            return;
        }

        _currentHoverMove = (targetParent, targetIndex);
        draggedItem.Parent?.Children?.Remove(draggedItem);
        targetParent.Children?.Insert(targetIndex, draggedItem);   
    }

    [RelayCommand]
    public void OnDrop(DropTargetEventArgs eventArgs)
    {
        if (_currentHoverMove is not var (targetItem, targetIndex) ||
            targetItem.CoreItem is not ILaminarStorageFolder targetFolder) return;
        if (eventArgs.DraggingControl.DataContext is not FileNavigatorItemViewModel draggedItem) return;

        fileBrowser.Move(draggedItem.CoreItem, targetFolder, targetIndex);
        _currentHoverMove = null;
        _proposedHoveredItem = null;
    }

    private static (FileNavigatorItemViewModel draggedItem, FileNavigatorItemViewModel targetParent, int targetIndex)?
        GetMoveFromDragInfo(DropTargetEventArgs eventArgs)
    {
        if (eventArgs.ReceptacleTag is not TreeViewDropAcceptor.TreeViewItemReceptacleInfo
            {
                ReceptacleParentDataContext: FileNavigatorItemViewModel targetParent,
                ReceptacleIndex: var targetIndex
            })
        {
            return null;
        }
        
        if (eventArgs.DraggingControl.DataContext is not FileNavigatorItemViewModel draggedItem) return null;

        return (draggedItem, targetParent, targetIndex);
    }
    
    private static bool IsValidMove(FileNavigatorItemViewModel draggedItem, FileNavigatorItemViewModel targetParent,
        int targetIndex)
    {
        if (targetParent.Children is null) return false;
        
        int? indexInParent = draggedItem.Parent?.Children?.IndexOf(draggedItem); 
        
        // The parent does not contain its child, or this hover does not constitute a move
        if (indexInParent == -1 || (indexInParent == targetIndex && Equals(draggedItem.Parent, targetParent))) return false;
        
        // Trying to move a folder into itself
        if (Equals(draggedItem, targetParent)) return false;

        // A list with 5 elements cannot have one of its own elements moved to index 5
        if (Equals(draggedItem.Parent, targetParent) && targetIndex == draggedItem.Parent?.Children?.Count) return false;

        return true;
    }
}