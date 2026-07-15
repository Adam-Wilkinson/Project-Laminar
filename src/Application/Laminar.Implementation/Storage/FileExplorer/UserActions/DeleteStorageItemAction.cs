using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.Implementation.Storage.FileExplorer.Infrastructure;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct DeleteStorageItemAction(IFileSystemItem item, FileBrowserActionDependencies dependencies) 
    : IUserAction
{
    private readonly CompoundAction _internalAction = new(
        new RenameStorageItemAction(GetDeletedName(dependencies.FileSystem.GetNameWithoutExtension(item.Path)), item, dependencies), 
        new MoveStorageItemAction(item, dependencies.Graph.RecyclingBin, null, dependencies));

    public IFileSystemItem Target => item;
    
    public bool CanExecute => _internalAction.CanExecute;

    public Task<IUserActionResult> Execute()
    {
        item.Refresh();
        
        if (item is IFileSystemRootFolder rootFolder)
        {
            CompoundAction? action = _internalAction;
            FileBrowserActionDependencies actionDependencies = dependencies;
            return Task.FromResult<IUserActionResult>(new ResolvableError<DeleteRootFolderConfirmation>
            {
                Exception = new DeleteRootFolderException(rootFolder.Path),
                Resolve = confirmation => confirmation switch
                {
                    DeleteRootFolderConfirmation.DeleteRootFolder => new AlternativeActionFound(action),
                    DeleteRootFolderConfirmation.RemoveRootFolder => new AlternativeActionFound(new RemoveRootFolderAction(rootFolder.Path, false, actionDependencies)),
                    DeleteRootFolderConfirmation.RemoveRootFolderAndCleanup => new AlternativeActionFound(new RemoveRootFolderAction(rootFolder.Path, true, actionDependencies)),
                    _ => throw new InvalidOperationException()
                }
            });
        }
        
        var executionResult = _internalAction.Execute();

        if (executionResult.Result is UserActionSuccess && item is IMutableFileSystemItem itemInternal)
        {
            itemInternal.OnDeleted();
        }
        
        
        return executionResult;
    }
    
    private static string GetDeletedName(string name) => $"({DateTime.UtcNow.Ticks}) {name}";
}
