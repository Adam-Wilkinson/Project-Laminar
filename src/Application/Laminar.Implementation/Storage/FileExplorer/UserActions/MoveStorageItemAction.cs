using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal class MoveStorageItemAction(
     LaminarStorageItem item, 
     ILaminarStorageFolder destinationFolder,
     int? targetIndex,
     FileExplorerActionDependencies dependencies)
     : IUserAction
{
     public event EventHandler? CanExecuteChanged { add { } remove { } }
     
     public bool CanExecute => true;
     
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
                return Task.FromResult<IUserActionResult>(new ResolvableError<NamingConflictResolution> 
                {
                    Exception = new DestinationContainsItemOfThatNameException(destinationFolder.UserFriendlyName, item.UserFriendlyName),
                    Resolve = resolution => resolution switch
                    {
                        NamingConflictResolution.ReplaceItem => new AlternativeActionFound(new CompoundAction(new DeleteStorageItemAction(internalItem, dependencies), this)),
                        NamingConflictResolution.IncrementName => new AlternativeActionFound(new CompoundAction(
                            new RenameStorageItemAction(dependencies.FileSystem.GetNameWithoutExtension(item.Path) + " (1)", item, dependencies), this)),
                        _ => throw new InvalidOperationException(),
                    },
                    OnCancelled = () =>
                    {
                        oldFolder.Refresh();
                        destinationFolder.Refresh();
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