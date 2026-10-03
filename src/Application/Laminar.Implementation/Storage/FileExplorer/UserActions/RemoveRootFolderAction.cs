using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct RemoveRootFolderAction(
    FileSystemPath rootFolderPath,
    bool fullyCleanup,
    FileBrowserActionDependencies dependencies) : IUserAction
{
    public FileSystemPath RootFolderPath => rootFolderPath;

    public Task<IUserActionExecutionOutcome> Execute()
    {
        dependencies.Graph.Roots.RemoveRootAt(rootFolderPath, fullyCleanup);
        return Task.FromResult(IUserActionExecutionOutcome.Success(new AddRootFolderAction(rootFolderPath, dependencies)));
    }
}