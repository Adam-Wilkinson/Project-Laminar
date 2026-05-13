using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.DragDrop;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Point = Avalonia.Point;

namespace Laminar.Avalonia.ViewModels;

public partial class FileNavigatorViewModel(
    ILaminarFileBrowser fileBrowser,
    DialogService dialogService,
    Func<ILaminarStorageItem, FileNavigatorItemViewModel> fileNavigatorItemViewModelFactory)
    : DropTargetViewModel
{
    private static readonly TimeSpan ExpandHoveredOverFolderDelay = new(0, 0, 0, 0, 500);

    private FileNavigatorItemViewModel? _proposedHoveredItem;
    private (FileNavigatorItemViewModel moveItem, int moveIndex)? _currentHoverMove;

    public IReadOnlyObservableCollection<FileNavigatorItemViewModel> RootFiles { get; set; } = 
        fileBrowser.RootFolders.ObservableMap(x =>
        {
            var result = fileNavigatorItemViewModelFactory(x);
            result.IsExpanded = true;
            return result;
        });
    
    [RelayCommand]
    private void Refresh()
    {
        foreach (var root in RootFiles)
        {
            root.Refresh();
        }
    }

    [RelayCommand]
    private async Task AddRootFolder()
    {
        FileSystemPath? possibleFolder = await dialogService.PromptForFolder(WellKnownFolder.Documents);
        if (possibleFolder is not { } selectedFolder) return;
        await fileBrowser.AddRootFolder(selectedFolder);
    }

    public override bool HoverEnter(object? payload, Point location, object? receptacleTag)
    {
        if (payload is not FileNavigatorItemViewModel draggedItem) return false;
        if (receptacleTag is not TreeViewDropAcceptor.TreeViewItemReceptacleInfo
            {
                ReceptacleParentDataContext: FileNavigatorItemViewModel targetParent,
                ReceptacleIndex: var targetIndex
            })
        {
            return false;
        }
        
        if (!IsValidMove(draggedItem, targetParent, targetIndex)) return false;

        if (!targetParent.IsExpanded)
        {
            if (Equals(_proposedHoveredItem, targetParent)) return true;
            
            _proposedHoveredItem = targetParent;
            Dispatcher.UIThread.InvokeAsync(async () => {
                await Task.Delay(ExpandHoveredOverFolderDelay);
                if (!Equals(_proposedHoveredItem, targetParent)) return;
                targetParent.IsExpanded = true;
                HoverEnter(draggedItem, location, receptacleTag);
            });

            return true;
        }
        
        _currentHoverMove = (targetParent, targetIndex);
        if (draggedItem.Parent is null) return false;
        
        draggedItem.Parent.Children?.Remove(draggedItem);
        targetParent.Children?.Insert(targetIndex, draggedItem);
        return true;

    }

    public override bool Drop(object? payload, Point location, object? receptacleTag)
    {
        if (_currentHoverMove is not var (targetItem, targetIndex) ||
            targetItem.CoreItem is not ILaminarStorageFolder targetFolder) return false;
        if (payload is not FileNavigatorItemViewModel draggedItem) return false;
        
        if (draggedItem.CoreItem is not null)
            _ = fileBrowser.Move(draggedItem.CoreItem, targetFolder, targetIndex);
        
        _currentHoverMove = null;
        _proposedHoveredItem = null;
        return true;
    }

    private static bool IsValidMove(FileNavigatorItemViewModel draggedItem, FileNavigatorItemViewModel targetParent, int targetIndex)
    {
        if (targetParent.Children is null || draggedItem.Parent is null) return false;
        
        int? indexInParent = draggedItem.Parent.Children?.IndexOf(draggedItem); 
        
        // The parent does not contain its child, or this hover does not constitute a move
        if (indexInParent == -1 || (indexInParent == targetIndex && Equals(draggedItem.Parent, targetParent))) return false;
        
        // Trying to move a folder into itself
        if (Equals(draggedItem, targetParent)) return false;

        // A list with 5 elements cannot have one of its own elements moved to index 5
        if (Equals(draggedItem.Parent, targetParent) && targetIndex == draggedItem.Parent?.Children?.Count) return false;

        return true;
    }
}