namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionScope
{
    public IUserActionSession BeginSession();

    public IUserActionScope CreateChild(params IUserActionSimplifier[] simplifiers);
    
    public Task<UserActionResult> ExecuteAction(IUserAction action);
    
    public Task<UserActionResult> Undo();

    public Task<UserActionResult> Redo();
}