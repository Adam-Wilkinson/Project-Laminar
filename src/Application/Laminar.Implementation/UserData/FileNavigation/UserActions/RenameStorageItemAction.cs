using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.UserData.FileNavigation.UserActions;

internal class RenameStorageItemAction(
    string newName, 
    LaminarStorageItem item, 
    ILaminarStorageRootFolder recyclingBin) : IUserAction
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }

    public bool CanExecute { get; } = item.Path.Name != newName;

    public Task<IUserActionResult> Execute()
    {
        if (item.ParentFolder is not { } parentFolder || Equals(item.Path.Name, newName))
        {
            return Task.FromResult(IUserActionResult.Invalid());
        }

        if (newName.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return Task.FromResult(IUserActionResult.Error(new InvalidStorageItemNameException(newName)));
        }

        if (parentFolder.Contents.FirstOrDefault(sibling => newName.Equals(sibling.Path.Name, StringComparison.OrdinalIgnoreCase)) is { } clash)
        {
            if (clash is not LaminarStorageItem internalItem) return Task.FromResult(IUserActionResult.Error(new InvalidOperationException("Clash with an item of a type I cannot handle")));
            return Task.FromResult<IUserActionResult>(new ResolvableError<NamingConflictResolution>
            {
                Exception = new FileWithNameExistsException(newName),
                Resolve = resolution => resolution switch
                {
                    NamingConflictResolution.IncrementName => new AlternativeActionFound(new RenameStorageItemAction(newName + " (1)", item, recyclingBin)),
                    NamingConflictResolution.ReplaceItem => new AlternativeActionFound(new CompoundAction(new DeleteStorageItemAction(internalItem, recyclingBin), this)),
                    _ => throw new InvalidOperationException(),
                },
                OnCancelled = () => item.Refresh(),
            });
        }
        
        var oldName = item.Path.Name;

        try
        {
            item.Rename(newName + item.Path.Extension);
        }
        catch (IOException exception)
        {
            return Task.FromResult(IUserActionResult.Error(exception));
        }
        
        return Task.FromResult(IUserActionResult.Success(new RenameStorageItemAction(oldName, item, recyclingBin)));
    }
}