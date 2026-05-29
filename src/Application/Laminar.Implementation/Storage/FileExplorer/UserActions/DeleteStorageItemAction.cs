using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct DeleteStorageItemAction(LaminarStorageItem item, FileExplorerActionDependencies dependencies) 
    : IUserAction
{
    private readonly CompoundAction _internalAction = new(
        new RenameStorageItemAction(GetDeletedName(dependencies.FileSystem.GetNameWithoutExtension(item.Path)), item, dependencies), 
        new MoveStorageItemAction(item, dependencies.RecyclingBin, null, dependencies));

    public bool CanExecute => _internalAction.CanExecute;

    public Task<IUserActionResult> Execute()
    {
        item.Refresh();
        
        if (item is ILaminarStorageRootFolder rootFolder)
        {
            CompoundAction? action = _internalAction;
            FileExplorerActionDependencies actionDependencies = dependencies;
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
        
        return _internalAction.Execute();
    }

    public IUserActionSimplification GetSimplificationAfter(IUserAction previousAction)
        => IUserActionSimplification.None();
    
    private static string GetDeletedName(string name) => $"({DateTime.UtcNow.Ticks}) {name}";
}
