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

        if (newName.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return IUserActionResult.Error(new InvalidStorageItemNameException(newName));
        }
        
        var oldName = item.Name;

        try
        {
            fileSystem.Move(item.Path, Path.Combine(item.ParentFolder.Path, newName + item.Extension));
        }
        catch (IOException exception)
        {
            return IUserActionResult.Error(exception);
        }
        
        return IUserActionResult.Success(new RenameStorageItemAction(oldName, item, fileSystem));
    }
}

public class InvalidStorageItemNameException(string name) : Exception
{
    public string Name => name;
}