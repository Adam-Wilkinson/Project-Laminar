using System;
using System.Linq;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;
using Laminar.Implementation.UserData.FileNavigation.Exceptions;

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
     
     public IUserActionResult Execute()
     {
          if (item.ParentFolder is not { } oldFolder || item.Path is not { } oldPath || destinationFolder.Path is not { } destinationFolderPath) 
               return IUserActionResult.Failure();
          
          var indexInOldFolder = oldFolder.Contents.IndexOf(item);
          var indexInDestinationFolder = targetIndex ?? destinationFolder.Contents.Count;
          
          if (Equals(oldFolder, destinationFolder) && destinationFolder.Contents is IObservableCollection<ILaminarStorageItem> editableCollection)
          {
               editableCollection.Move(indexInOldFolder, indexInDestinationFolder);
          }
          else
          {
               if (destinationFolder.Contents.Any(x => oldPath.Name.Equals(x.Path?.Name)))
               {
                    return IUserActionResult.Error(
                         new DestinationContainsItemOfThatNameException(destinationFolderPath.Name, oldPath.Name));
               }
               
               // The file system is not positional, so we prep the StorageFolder to move to the right position
               if (destinationFolder is LaminarStorageFolder typedFolder)
               {
                    typedFolder.RegisterQueuedMove(item, indexInDestinationFolder);
               }

               var destinationPath = destinationFolderPath.ChildPath(oldPath.NameAndExtension);
               fileSystem.Move(oldPath, destinationPath);
          }

          return IUserActionResult.Success(new MoveStorageItemAction(item, oldFolder, fileSystem, indexInOldFolder));
     }
}