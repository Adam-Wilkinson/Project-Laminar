using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Workspaces.Actions;

public readonly struct CreateWorkspaceAction(
    IPersistentValue<List<FileSystemPath>> allPaths,
    FileSystemPath newWorkspacePath) : IUserAction
{
    public FileSystemPath NewWorkspacePath => newWorkspacePath;
    
    public Task<IUserActionExecutionOutcome> Execute()
    {
        allPaths.Value = [.. allPaths.Value, newWorkspacePath];
        return Task.FromResult(IUserActionExecutionOutcome.Success(new RemoveWorkspaceAction()));
    }
}