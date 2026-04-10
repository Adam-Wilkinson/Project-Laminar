namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionManager
{
    public IUserActionResult ExecuteAction(IUserAction action);
    
    public Task<IUserActionResult> ExecuteActionAsync(IUserAction action);

    public IUserActionResult Undo();

    public Task<IUserActionResult> UndoAsync();

    public IUserActionResult Redo();

    public Task<IUserActionResult> RedoAsync();
}
