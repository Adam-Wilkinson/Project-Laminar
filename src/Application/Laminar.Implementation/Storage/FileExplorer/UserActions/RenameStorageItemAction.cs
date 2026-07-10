using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct RenameStorageItemAction(
    string newName, 
    IFileSystemItem item, 
    FileBrowserActionDependencies dependencies) : IUserAction
{
    public IFileSystemItem Target => item;
    
    public bool CanExecute { get; } = !dependencies.FileSystem.GetNameWithoutExtension(item.Path).Equals(newName);

    public Task<IUserActionResult> Execute()
    {
        var oldName = dependencies.FileSystem.GetNameWithoutExtension(item.Path);
        var itemExtension = dependencies.FileSystem.GetExtension(item.Path);

        if (Equals(oldName, newName))
        {
            return Task.FromResult(IUserActionResult.Ineffectual());
        }
        
        if (item.ParentFolder is not { } parentFolder)
        {
            return Task.FromResult(IUserActionResult.Error(new InvalidOperationException("The storage item does not have a parent")));
        }

        if (newName.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return Task.FromResult(IUserActionResult.Error(new InvalidStorageItemNameException(newName)));
        }

        string name = newName;
        FileBrowserActionDependencies actionDependencies = dependencies;
        
        if (parentFolder.LoadOrGetContents().FirstOrDefault(sibling => name.Equals(
                actionDependencies.FileSystem.GetNameWithoutExtension(sibling.Path), FileSystemPath.RuntimeStringComparison)) 
            is { } clash)
        {
            RenameStorageItemAction renameAction = this;
            IFileSystemItem targetItem = item;
            
            return Task.FromResult<IUserActionResult>(new ResolvableError<NamingConflictResolution>
            {
                Exception = new FileWithNameExistsException(newName),
                Resolve = resolution => resolution switch
                {
                    NamingConflictResolution.IncrementName => new AlternativeActionFound(new RenameStorageItemAction(name + " (1)", targetItem, actionDependencies)),
                    NamingConflictResolution.ReplaceItem => new AlternativeActionFound(new CompoundAction(new DeleteStorageItemAction(clash, actionDependencies), renameAction)),
                    _ => throw new InvalidOperationException(),
                },
                OnCancelled = item.Refresh,
            });
        }
        
        try
        {
            dependencies.Graph.Rename(item, newName + itemExtension);
        }
        catch (IOException exception)
        {
            return Task.FromResult(IUserActionResult.Error(exception));
        }
        
        return Task.FromResult(IUserActionResult.Success(new RenameStorageItemAction(oldName, item, dependencies)));
    }
}