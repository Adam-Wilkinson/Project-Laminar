namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionSessionHost
{
    public Task<IUserActionResult> ResolveExecutionAsync(IUserAction action);

    public void RegisterUndoAction(IUserAction action);

    public void Simplify(List<IUserAction> actions);
}