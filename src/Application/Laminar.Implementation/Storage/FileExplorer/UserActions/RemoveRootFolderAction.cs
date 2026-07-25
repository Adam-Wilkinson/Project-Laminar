using Laminar.Contracts.Base.ActionSystem;
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
        dependencies.Graph.Roots.RemoveRootAt(rootFolderPath, fullyCleanup);
        return Task.FromResult(IUserActionResult.Success(new AddRootFolderAction(rootFolderPath, dependencies)));
    }
}