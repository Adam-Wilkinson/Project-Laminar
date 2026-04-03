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
    
    public bool CanExecute { get; } = item.Path.HasValue && item.Path.Value.Name != newName;

    public IUserActionResult Execute()
    {
        if (item.ParentFolder is not { Path: { } parentPath } parentFolder || item.Path is not { } itemPath 
                                                                           || Equals(itemPath.Name, newName))
        {
            return IUserActionResult.Failure();
        }

        if (newName.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return IUserActionResult.Error(new InvalidStorageItemNameException(newName));
        }

        if (parentFolder.Contents.Any(sibling => newName.Equals(sibling.Path?.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return IUserActionResult.Error(new FileWithNameExistsException(newName));
        }
        
        var oldName = itemPath.Name;

        try
        {
            fileSystem.Move(itemPath, parentPath.ChildPath(newName + itemPath.Extension));
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