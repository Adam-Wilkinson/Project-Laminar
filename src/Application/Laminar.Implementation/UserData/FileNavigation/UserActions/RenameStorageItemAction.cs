using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class RenameStorageItemAction(string newName, ILaminarStorageItem item, IFileSystem fileSystem) : IUserAction
{
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute { get; } = item.Path.Name != newName;

    public async Task<IUserActionResult> Execute()
    {
        if (item.ParentFolder is not { Path: { } parentPath } parentFolder || Equals(item.Path.Name, newName))
        {
            return IUserActionResult.Invalid();
        }

        if (newName.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return IUserActionResult.Error(new InvalidStorageItemNameException(newName));
        }

        if (parentFolder.Contents.Any(sibling => newName.Equals(sibling.Path.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return IUserActionResult.Error(new FileWithNameExistsException(newName));
        }
        
        var oldName = item.Path.Name;

        try
        {
            fileSystem.Move(item.Path, parentPath.ChildPath(newName + item.Path.Extension));
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