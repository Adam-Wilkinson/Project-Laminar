using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal class RemoveRootFolderAction(
    FileSystemPath rootFolderPath,
    bool fullyCleanup,
    FileExplorerActionDependencies dependencies) : IUserAction
{
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

    public bool IsInverseOf(IUserAction action) => false;
}