using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct AddRootFolderAction(
    FileSystemPath folderPath, 
    FileBrowserActionDependencies dependencies) : IUserAction
{
    public bool CanExecute => true;
    
    public FileSystemPath RootFolderPath => folderPath;
    
    public Task<IUserActionResult> Execute()
    {
        dependencies.Graph.Roots.AddRoot(RootFolderPath);
        return Task.FromResult(IUserActionResult.Success(new RemoveRootFolderAction(folderPath, true, dependencies)));
    }
}