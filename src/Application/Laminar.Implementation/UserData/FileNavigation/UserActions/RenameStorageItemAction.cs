using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

public class RenameStorageItemAction(string newName, ILaminarStorageItem item, IFileSystem fileSystem, ILaminarStorageRootFolder recyclingBin) : IUserAction
{
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute { get; } = item.Path.Name != newName;

    public Task<IUserActionResult> Execute()
    {
        if (item.ParentFolder is not { Path: { } parentPath } parentFolder || Equals(item.Path.Name, newName))
        {
            return Task.FromResult(IUserActionResult.Invalid());
        }

        if (newName.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return Task.FromResult(IUserActionResult.Error(new InvalidStorageItemNameException(newName)));
        }

        if (parentFolder.Contents.FirstOrDefault(sibling => newName.Equals(sibling.Path.Name, StringComparison.OrdinalIgnoreCase)) is { } clash)
        {
            item.Refresh();
            return Task.FromResult<IUserActionResult>(new ResolvableError<NamingConflictResolution>
            {
                Exception = new FileWithNameExistsException(newName),
                Resolve = resolution => resolution switch
                {
                    NamingConflictResolution.IncrementName => new AlternativeActionFound(new RenameStorageItemAction(newName + " (1)", item, fileSystem, recyclingBin)),
                    NamingConflictResolution.ReplaceItem => new AlternativeActionFound(new CompoundAction(new DeleteStorageItemAction(clash, fileSystem, recyclingBin), this)),
                    _ => throw new InvalidOperationException(),
                }
            });
        }
        
        var oldName = item.Path.Name;

        try
        {
            fileSystem.Move(item.Path, parentPath.ChildPath(newName + item.Path.Extension));
            (item as LaminarStorageItem)?.Rename(newName + item.Path.Extension);
        }
        catch (IOException exception)
        {
            item.Refresh();
            return Task.FromResult(IUserActionResult.Error(exception));
        }
        
        return Task.FromResult(IUserActionResult.Success(new RenameStorageItemAction(oldName, item, fileSystem, recyclingBin)));
    }
}