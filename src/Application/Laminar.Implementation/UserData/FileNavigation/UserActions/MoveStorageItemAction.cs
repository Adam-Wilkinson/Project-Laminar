using System;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;

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
     
     public UserActionResult Execute()
     {
          if (item.ParentFolder is not { } oldFolder || item is not LaminarStorageItem storageItem) 
               return UserActionResult.Failure();
          
          var indexInOldFolder = oldFolder.Contents.IndexOf(item);
          var indexInDestinationFolder = targetIndex ?? destinationFolder.Contents.Count;
          
          if (Equals(oldFolder, destinationFolder) && destinationFolder.Contents is IObservableCollection<ILaminarStorageItem> editableCollection)
          {
               editableCollection.Move(indexInOldFolder, indexInDestinationFolder);
          }
          else
          {
               // The file system is not positional, so we prep the StorageFolder to move to the right position
               if (destinationFolder is LaminarStorageFolder typedFolder)
               {
                    typedFolder.RegisterQueuedMove(item, indexInDestinationFolder);
               }

               
               
               var destinationPath = System.IO.Path.Join(destinationFolder.Path, item.Name + item.Extension);
               fileSystem.Move(storageItem.FileSystemInfo, destinationPath);
               oldFolder.Refresh();
               destinationFolder.Refresh();
               item.Refresh();
          }

          return UserActionResult.Success(new MoveStorageItemAction(item, oldFolder, fileSystem, indexInOldFolder));
     }
}