using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal class RenameStorageItemAction(
    string newName, 
    LaminarStorageItem item, 
    FileExplorerActionDependencies dependencies) : IUserAction
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }

    public bool CanExecute { get; } = !dependencies.FileSystem.GetNameWithoutExtension(item.Path).Equals(newName);

    public Task<IUserActionResult> Execute()
    {
        var oldName = dependencies.FileSystem.GetNameWithoutExtension(item.Path);
        var itemExtension = dependencies.FileSystem.GetExtension(oldName);
        
        if (item.ParentFolder is not { } parentFolder || Equals(oldName, newName))
        {
            return Task.FromResult(IUserActionResult.Invalid());
        }

        if (newName.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return Task.FromResult(IUserActionResult.Error(new InvalidStorageItemNameException(newName)));
        }

        if (parentFolder.Contents.FirstOrDefault(sibling => newName.Equals(
                dependencies.FileSystem.GetNameWithoutExtension(sibling.Path), FileSystemPath.RuntimeStringComparison)) 
            is { } clash)
        {
            if (clash is not LaminarStorageItem internalItem) 
                return Task.FromResult(IUserActionResult.Error(new InvalidOperationException("Clash with an item of a type I cannot handle")));
            return Task.FromResult<IUserActionResult>(new ResolvableError<NamingConflictResolution>
            {
                Exception = new FileWithNameExistsException(newName),
                Resolve = resolution => resolution switch
                {
                    NamingConflictResolution.IncrementName => new AlternativeActionFound(new RenameStorageItemAction(newName + " (1)", item, dependencies)),
                    NamingConflictResolution.ReplaceItem => new AlternativeActionFound(new CompoundAction(new DeleteStorageItemAction(internalItem, dependencies), this)),
                    _ => throw new InvalidOperationException(),
                },
                OnCancelled = item.Refresh,
            });
        }
        
        try
        {
            item.Rename(newName + itemExtension);
        }
        catch (IOException exception)
        {
            return Task.FromResult(IUserActionResult.Error(exception));
        }
        
        return Task.FromResult(IUserActionResult.Success(new RenameStorageItemAction(oldName, item, dependencies)));
    }
}