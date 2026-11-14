using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Workspaces.Actions;

public readonly struct RemoveWorkspaceAction(
    IPersistentValue<List<FileSystemPath>> allPaths,
    FileSystemPath pathToRemove) : IUserAction
{
    public Task<IUserActionExecutionOutcome> Execute()
    {
        
    }
}