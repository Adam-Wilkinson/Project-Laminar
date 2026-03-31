using System;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class DeleteStorageItemAction<T>(IFileSystem fileSystem, T item) : IUserAction where T : class, ILaminarStorageItem
{
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute => item.ParentFolder is not null && item is LaminarStorageItem;
    
    public IUserActionResult Execute()
    {
        if (item is not LaminarStorageItem storageItem || item.ParentFolder is not { } parentFolder) 
            return IUserActionResult.Failure();

        int indexInParent = parentFolder.Contents.IndexOf(item);
        fileSystem.Delete(storageItem.Path);
        
        storageItem.ParentFolder?.Refresh();
        return IUserActionResult.Success(new InsertStorageItemAction<T>(fileSystem, item, parentFolder, indexInParent));
    }
}