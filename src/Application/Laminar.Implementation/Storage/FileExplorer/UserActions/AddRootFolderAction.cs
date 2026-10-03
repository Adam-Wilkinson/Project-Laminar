using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct AddRootFolderAction(
    FileSystemPath folderPath, 
    FileBrowserActionDependencies dependencies) : IUserAction
{
    public FileSystemPath RootFolderPath => folderPath;
    
    public Task<IUserActionExecutionOutcome> Execute()
    {
        dependencies.Graph.Roots.EnsureRootRegistered(RootFolderPath);
        return Task.FromResult(IUserActionExecutionOutcome.Success(new RemoveRootFolderAction(folderPath, true, dependencies)));
    }
}