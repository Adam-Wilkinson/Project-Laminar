using System;
using System.Linq;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Domain.Notification;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class MoveStorageItemAction(
     ILaminarStorageItem item, 
     ILaminarStorageFolder destinationFolder,
     IFileSystem fileSystem,
     int? targetIndex = null)
     : IUserAction
{
     public event EventHandler? CanExecuteChanged;
     
     public bool CanExecute => true;
     
    public Task<IUserActionResult> Execute()
    {
        if (item.ParentFolder is not { } oldFolder) return Task.FromResult(IUserActionResult.Invalid());
        
        var indexInOldFolder = oldFolder.Contents.IndexOf(item);
        var indexInDestinationFolder = targetIndex ?? destinationFolder.Contents.Count;
         
        if (Equals(oldFolder, destinationFolder) && destinationFolder.Contents is IObservableCollection<ILaminarStorageItem> editableCollection)
        {
             editableCollection.Move(indexInOldFolder, indexInDestinationFolder);
        }
        else
        {
            if (destinationFolder.Contents.Any(x => item.Path.Name.Equals(x.Path.Name)))
            {
                oldFolder.Refresh();
                destinationFolder.Refresh();
                return Task.FromResult<IUserActionResult>(new ResolvableError<NamingConflictResolution> 
                {
                    Exception = new DestinationContainsItemOfThatNameException(destinationFolder.Path.Name, item.Path.Name),
                    Resolve = resolution => resolution switch
                    {
                        NamingConflictResolution.ReplaceItem => new AlternativeActionFound(new CompoundAction(new DeleteStorageItemAction(), this)),
                        NamingConflictResolution.IncrementName => new AlternativeActionFound(new CompoundAction(new RenameStorageItemAction(item.Path.Name + " (1)", item, fileSystem), this)),
                        _ => throw new InvalidOperationException(),
                    }
                });
            }

            // The file system is not positional, so we prep the StorageFolder to move to the right position
            if (destinationFolder is LaminarStorageFolder typedFolder)
            {
               typedFolder.RegisterQueuedMove(item, indexInDestinationFolder);
            }

            var destinationPath = destinationFolder.Path.ChildPath(item.Path.NameAndExtension);
            fileSystem.Move(item.Path, destinationPath);
        }
        return Task.FromResult(IUserActionResult.Success(new MoveStorageItemAction(item, oldFolder, fileSystem, indexInOldFolder)));
    }
}