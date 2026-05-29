using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct RenameStorageItemAction(
    string newName, 
    LaminarStorageItem item, 
    FileExplorerActionDependencies dependencies) : IUserAction
{
    public LaminarStorageItem Target => item;
    
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

        string name = newName;
        FileExplorerActionDependencies actionDependencies = dependencies;
        
        if (parentFolder.Contents.FirstOrDefault(sibling => name.Equals(
                actionDependencies.FileSystem.GetNameWithoutExtension(sibling.Path), FileSystemPath.RuntimeStringComparison)) 
            is { } clash)
        {
            if (clash is not LaminarStorageItem internalItem) 
                return Task.FromResult(IUserActionResult.Error(new InvalidOperationException("Clash with an item of a type I cannot handle")));
            
            RenameStorageItemAction renameAction = this;
            LaminarStorageItem targetItem = item;
            
            return Task.FromResult<IUserActionResult>(new ResolvableError<NamingConflictResolution>
            {
                Exception = new FileWithNameExistsException(newName),
                Resolve = resolution => resolution switch
                {
                    NamingConflictResolution.IncrementName => new AlternativeActionFound(new RenameStorageItemAction(name + " (1)", targetItem, actionDependencies)),
                    NamingConflictResolution.ReplaceItem => new AlternativeActionFound(new CompoundAction(new DeleteStorageItemAction(internalItem, actionDependencies), renameAction)),
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

    public IUserActionSimplification GetSimplificationAfter(IUserAction previousAction)
    {
        if (previousAction is RenameStorageItemAction renameAction && renameAction.Target == Target)
        {
            return IUserActionSimplification.Overrides();
        }

        return IUserActionSimplification.None();
    }

    public bool IsInverseOf(IUserAction action) => false;
}