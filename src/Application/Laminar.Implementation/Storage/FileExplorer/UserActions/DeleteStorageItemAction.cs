using System;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal class DeleteStorageItemAction(LaminarStorageItem item, FileExplorerActionDependencies dependencies) 
    : IUserAction
{
    private readonly CompoundAction _internalAction = new(
        new RenameStorageItemAction(GetDeletedName(dependencies.FileSystem.GetNameWithoutExtension(item.Path)), item, dependencies), 
        new MoveStorageItemAction(item, dependencies.RecyclingBin, null, dependencies));

    public bool CanExecute => _internalAction.CanExecute;

    public event EventHandler? CanExecuteChanged
    {
        add => _internalAction.CanExecuteChanged += value;
        remove => _internalAction.CanExecuteChanged -= value;
    }

    public Task<IUserActionResult> Execute()
    {
        item.Refresh();
        
        if (item is ILaminarStorageRootFolder rootFolder)
        {
            return Task.FromResult<IUserActionResult>(new ResolvableError<DeleteRootFolderConfirmation>
            {
                Exception = new DeleteRootFolderException(rootFolder.Path),
                Resolve = confirmation => confirmation switch
                {
                    DeleteRootFolderConfirmation.DeleteRootFolder => new AlternativeActionFound(_internalAction),
                    DeleteRootFolderConfirmation.RemoveRootFolder => new AlternativeActionFound(new RemoveRootFolderAction(rootFolder.Path, false, dependencies)),
                    DeleteRootFolderConfirmation.RemoveRootFolderAndCleanup => new AlternativeActionFound(new RemoveRootFolderAction(rootFolder.Path, true, dependencies)),
                    _ => throw new InvalidOperationException()
                }
            });
        }
        
        return _internalAction.Execute();
    }

    private static string GetDeletedName(string name) => $"({DateTime.UtcNow.Ticks}) {name}";
}
