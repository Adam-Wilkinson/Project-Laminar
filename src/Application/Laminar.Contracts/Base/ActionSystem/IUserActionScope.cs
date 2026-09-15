namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionScope
{
    public IUserActionSession BeginSession();
    
    public Task<IUserActionResult> ExecuteAction(IUserAction action);
    
    public Task<IUserActionResult> Undo();

    public Task<IUserActionResult> Redo();
}