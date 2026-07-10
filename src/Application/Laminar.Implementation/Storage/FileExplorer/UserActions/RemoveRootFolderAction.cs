using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct RemoveRootFolderAction(
    FileSystemPath rootFolderPath,
    bool fullyCleanup,
    FileBrowserActionDependencies dependencies) : IUserAction
{
    public bool CanExecute => true;
    
    public FileSystemPath RootFolderPath => rootFolderPath;

    public Task<IUserActionResult> Execute()
    {
        if (!dependencies.ItemRepository.TryGetExisting(rootFolderPath, out var existing))
        {
            return Task.FromResult(IUserActionResult.Ineffectual());
        }

        if (existing is not IFileSystemRootFolder rootFolder)
        {
            return Task.FromResult(IUserActionResult.Error(new InvalidOperationException("Attempt to remove an item that is not a root folder")));
        }
        
        var currentList = new List<FileSystemPath>(dependencies.RootFolders.Value);
        currentList.Remove(rootFolderPath);
        dependencies.RootFolders.Value = currentList;
        rootFolder.Dispose(fullyCleanup);
        return Task.FromResult(IUserActionResult.Success(new AddRootFolderAction(rootFolderPath, dependencies)));
    }
}