using System;
using System.IO;
using System.Linq;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class RenameStorageItemAction(string newName, ILaminarStorageItem item, IFileSystem fileSystem) : IUserAction
{
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute { get; } = item.Path.Name != newName;

    public IUserActionResult Execute()
    {
        if (item.ParentFolder is null || Equals(item.Path.Name, newName))
        {
            return IUserActionResult.Failure();
        }

        if (newName.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return IUserActionResult.Error(new InvalidStorageItemNameException(newName));
        }

        if (item.ParentFolder.Contents.Any(sibling => sibling.Path.Name == newName))
        {
            return IUserActionResult.Error(new FileWithNameExistsException(newName));
        }
        
        var oldName = item.Path.Name;

        try
        {
            fileSystem.Move(item.Path, item.ParentFolder.Path.ChildPath(newName + item.Path.Extension));
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

public class FileWithNameExistsException(string name) : Exception
{
    public string Name => name;
}