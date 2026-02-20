using System;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class MoveStorageItemAction(
     ILaminarStorageItem item, 
     ILaminarStorageFolder destinationFolder, 
     int? targetIndex = null)
     : IUserAction
{
     public event EventHandler? CanExecuteChanged;
     
     public bool CanExecute => true;
     
     public IUserAction? Execute()
     {
          if (item.ParentFolder is null) return null;
          var oldFolder = item.ParentFolder;
          var indexInOldFolder = item.ParentFolder.Contents.IndexOf(item);
          var indexInDestinationFolder = targetIndex ?? destinationFolder.Contents.Count;

          if (Equals(oldFolder, destinationFolder))
          {
               destinationFolder.Contents.Move(indexInOldFolder, indexInDestinationFolder);
          }
          else
          {
               destinationFolder.Contents.Insert(indexInDestinationFolder, item);
          }

          return new MoveStorageItemAction(item, oldFolder, indexInOldFolder);
     }
}