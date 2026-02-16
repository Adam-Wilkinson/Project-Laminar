using System;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class MoveStorageItemAction(
     ILaminarStorageItem item, 
     ILaminarStorageFolder destinationFolder, 
     int? indexInFolder = null)
     : IUserAction
{
     public event EventHandler? CanExecuteChanged;
     
     public bool CanExecute => true;
     
     public IUserAction? Execute()
     {
          if (item.ParentFolder is null) return null;
          var oldFolder = item.ParentFolder;
          var indexInOldFolder = item.ParentFolder.Contents.IndexOf(item);
          if (indexInFolder.HasValue)
          {
               destinationFolder.Contents.Insert(indexInFolder.Value, item);
          }
          else
          {
               destinationFolder.Contents.Add(item);
          }

          return new MoveStorageItemAction(item, oldFolder, indexInOldFolder);
     }
}