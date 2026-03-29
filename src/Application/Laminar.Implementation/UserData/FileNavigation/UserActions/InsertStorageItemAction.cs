using System;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Notification;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class InsertStorageItemAction<T>(IFileSystem fileSystem, T item, ILaminarStorageFolder folder, int index) : IUserAction where T : class, ILaminarStorageItem
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute => true;
    public IUserActionResult Execute()
    {
        if (item is not LaminarStorageItem storageItem) return IUserActionResult.Failure();
        
        // We insert the item before working with the file system so it will be placed at the right index
        if (folder is LaminarStorageFolder destinationFolder)
        {
            destinationFolder.RegisterQueuedMove(item, index);
        }

        var destinationPath = System.IO.Path.Join(folder.Path, item.Name + item.Extension);
        fileSystem.Move(storageItem.FileSystemInfo, destinationPath);
        item.Refresh();
        item.ParentFolder?.Refresh();
        folder.Refresh();
        
        return IUserActionResult.Success(new DeleteStorageItemAction<T>(fileSystem, item));
    }
}