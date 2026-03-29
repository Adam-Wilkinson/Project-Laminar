using System;
using System.IO;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class RenameStorageItemAction(string newName, ILaminarStorageItem item, IFileSystem fileSystem) : IUserAction
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute { get; } = item.Name != newName;

    public IUserActionResult Execute()
    {
        if (item.ParentFolder is null || Equals(item.Name, newName))
        {
            return IUserActionResult.Failure();
        }
        
        string oldName = item.Name;
        fileSystem.Move(item.Path, Path.Combine(item.ParentFolder.Path, newName + item.Extension));
        return IUserActionResult.Success(new RenameStorageItemAction(oldName, item, fileSystem));
    }
}