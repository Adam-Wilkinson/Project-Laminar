namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionManager
{
    public Task<IUserActionResult> ExecuteAction(IUserAction action);

    public Task<IUserActionResult> Undo();

    public Task<IUserActionResult> Redo();
}
