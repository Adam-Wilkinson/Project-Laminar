using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Domain.Notification;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct MoveStorageItemAction(
     LaminarStorageItem item, 
     ILaminarStorageFolder destinationFolder,
     int? targetIndex,
     FileExplorerActionDependencies dependencies)
     : IUserAction
{
    public bool CanExecute => true;
     
    public LaminarStorageItem Target => item;
    
    public Task<IUserActionResult> Execute()
    {
        if (item is ILaminarStorageRootFolder)
            return Task.FromResult(IUserActionResult.Error(new CannotMoveRootFolderException(item.UserFriendlyName)));
        
        if (item.ParentFolder is not { } oldFolder || destinationFolder.Contents is not IObservableCollection<ILaminarStorageItem> destinationFolderContents) return Task.FromResult(IUserActionResult.Invalid());
        
        var indexInOldFolder = oldFolder.Contents.IndexOf(item);
        var indexInDestinationFolder = targetIndex ?? destinationFolder.Contents.Count;
         
        if (Equals(oldFolder, destinationFolder))
        {
            indexInDestinationFolder = Math.Min(indexInDestinationFolder, destinationFolderContents.Count);
            destinationFolderContents.Move(indexInOldFolder, indexInDestinationFolder);
        }
        else
        {
            if (destinationFolder.Contents.FirstOrDefault(NameEqualsItemName) is { } clash)
            {
                if (clash is not LaminarStorageItem internalItem) 
                    return Task.FromResult(IUserActionResult.Error(new InvalidOperationException("Clash with an item of a type I cannot handle")));
                
                FileExplorerActionDependencies actionDependencies = dependencies;
                MoveStorageItemAction moveAction = this;
                LaminarStorageItem targetItem = item;
                ILaminarStorageFolder destination = destinationFolder;
                
                return Task.FromResult<IUserActionResult>(new ResolvableError<NamingConflictResolution> 
                {
                    Exception = new DestinationContainsItemOfThatNameException(destinationFolder.UserFriendlyName, item.UserFriendlyName),
                    Resolve = resolution => resolution switch
                    {
                        NamingConflictResolution.ReplaceItem => new AlternativeActionFound(new CompoundAction(new DeleteStorageItemAction(internalItem, actionDependencies), moveAction)),
                        NamingConflictResolution.IncrementName => new AlternativeActionFound(new CompoundAction(
                            new RenameStorageItemAction(actionDependencies.FileSystem.GetNameWithoutExtension(targetItem.Path) + " (1)", targetItem, actionDependencies), moveAction)),
                        _ => throw new InvalidOperationException(),
                    },
                    OnCancelled = () =>
                    {
                        oldFolder.Refresh();
                        destination.Refresh();
                    }
                });
            }

            try
            {
                indexInDestinationFolder = Math.Min(indexInDestinationFolder, destinationFolderContents.Count);
                destinationFolderContents.Insert(indexInDestinationFolder, item);
            }
            catch (IOException exception)
            {
                return Task.FromResult(IUserActionResult.Error(exception));
            }
        }

        return Task.FromResult(IUserActionResult.Success(new MoveStorageItemAction(item, oldFolder, indexInOldFolder, dependencies)));
    }
    
    private bool NameEqualsItemName(ILaminarStorageItem comparisonItem)
        => item.UserFriendlyName.Equals(comparisonItem.UserFriendlyName, FileSystemPath.RuntimeStringComparison);
}