using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal class RemoveRootFolderAction(
    FileSystemPath rootFolderPath,
    bool fullyCleanup,
    FileExplorerActionDependencies dependencies) : IUserAction
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }

    public bool CanExecute => true;

    public Task<IUserActionResult> Execute()
    {
        if (dependencies.StorageItemFactory.TryGetExisting(rootFolderPath) is not LaminarStorageRootFolder rootFolder)
            return Task.FromResult(IUserActionResult.Invalid());
        
        var currentList = new List<FileSystemPath>(dependencies.RootFolders.Value);
        currentList.Remove(rootFolderPath);
        dependencies.RootFolders.Value = currentList;
        rootFolder.Dispose(fullyCleanup);
        return Task.FromResult(IUserActionResult.Success(new AddRootFolderAction(rootFolderPath, dependencies)));
    }
}