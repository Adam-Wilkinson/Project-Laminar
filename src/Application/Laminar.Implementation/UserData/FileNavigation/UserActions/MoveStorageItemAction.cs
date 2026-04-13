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
     ILaminarStorageRootFolder recyclingBin,
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
            indexInDestinationFolder = Math.Min(indexInDestinationFolder, editableCollection.Count);
            editableCollection.Move(indexInOldFolder, indexInDestinationFolder);
        }
        else
        {
            if (destinationFolder.Contents.FirstOrDefault(x => item.Path.Name.Equals(x.Path.Name, StringComparison.OrdinalIgnoreCase)) is { } clash)
            {
                oldFolder.Refresh();
                destinationFolder.Refresh();
                return Task.FromResult<IUserActionResult>(new ResolvableError<NamingConflictResolution> 
                {
                    Exception = new DestinationContainsItemOfThatNameException(destinationFolder.Path.Name, item.Path.Name),
                    Resolve = resolution => resolution switch
                    {
                        NamingConflictResolution.ReplaceItem => new AlternativeActionFound(new CompoundAction(new DeleteStorageItemAction(clash, fileSystem, recyclingBin), this)),
                        NamingConflictResolution.IncrementName => new AlternativeActionFound(new CompoundAction(new RenameStorageItemAction(item.Path.Name + " (1)", item, fileSystem, recyclingBin), this)),
                        _ => throw new InvalidOperationException(),
                    }
                });
            }

            var destinationPath = destinationFolder.Path.ChildPath(item.Path.NameAndExtension);

            try
            {
                fileSystem.Move(item.Path, destinationPath);
                if (destinationFolder.Contents is IObservableCollection<ILaminarStorageItem> destinationFolderEditable)
                {
                    indexInDestinationFolder = Math.Min(indexInDestinationFolder, destinationFolderEditable.Count);
                    destinationFolderEditable.Insert(indexInDestinationFolder, item);
                }
            }
            catch (IOException exception)
            {
                return Task.FromResult(IUserActionResult.Error(exception));
            }
        }

        return Task.FromResult(IUserActionResult.Success(new MoveStorageItemAction(item, oldFolder, fileSystem, recyclingBin, indexInOldFolder)));
    }
}