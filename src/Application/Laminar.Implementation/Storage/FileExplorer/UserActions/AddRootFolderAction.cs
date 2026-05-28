using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

public class AddRootFolderAction(FileSystemPath folderPath, FileExplorerActionDependencies dependencies) : IUserAction
{
    public bool CanExecute => true;
    
    public Task<IUserActionResult> Execute()
    {
        var currentList = new List<FileSystemPath>(dependencies.RootFolders.Value) { folderPath };
        dependencies.RootFolders.Value = currentList;
        return Task.FromResult(IUserActionResult.Success(new RemoveRootFolderAction(folderPath, true, dependencies)));
    }

    public bool IsInverseOf(IUserAction action) => false;
}