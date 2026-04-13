using System;
using System.IO;
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
            if (destinationFolder.Contents.Any(x => item.Path.Name.Equals(x.Path.Name, StringComparison.OrdinalIgnoreCase)))
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

            var destinationPath = destinationFolder.Path.ChildPath(item.Path.NameAndExtension);

            try
            {
                fileSystem.Move(item.Path, destinationPath);
                (destinationFolder.Contents as IObservableCollection<ILaminarStorageItem>)?.Insert(indexInDestinationFolder, item);
            }
            catch (IOException exception)
            {
                return Task.FromResult(IUserActionResult.Error(exception));
            }
        }

        return Task.FromResult(IUserActionResult.Success(new MoveStorageItemAction(item, oldFolder, fileSystem, indexInOldFolder)));
    }
}