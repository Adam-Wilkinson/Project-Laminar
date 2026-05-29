using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct AddRootFolderAction(
    FileSystemPath folderPath, 
    FileExplorerActionDependencies dependencies) : IUserAction
{
    public bool CanExecute => true;
    
    public FileSystemPath RootFolderPath => folderPath;
    
    public Task<IUserActionResult> Execute()
    {
        var currentList = new List<FileSystemPath>(dependencies.RootFolders.Value) { folderPath };
        dependencies.RootFolders.Value = currentList;
        return Task.FromResult(IUserActionResult.Success(new RemoveRootFolderAction(folderPath, true, dependencies)));
    }

    public IUserActionSimplification GetSimplificationAfter(IUserAction previousAction)
    {
        if (previousAction is RemoveRootFolderAction removeAction && removeAction.RootFolderPath == RootFolderPath)
        {
            return IUserActionSimplification.Undoes();
        }

        return IUserActionSimplification.None();
    }
}