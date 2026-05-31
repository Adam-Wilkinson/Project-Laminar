namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionManager
{
    public IUserActionSession BeginSession();
    
    public Task<IUserActionResult> ExecuteAction(IUserAction action);
    
    public Task<IUserActionResult> Undo();

    public Task<IUserActionResult> Redo();

    public IUserActionManager RegisterSimplifier(IUserActionSimplifier simplifier);
}
