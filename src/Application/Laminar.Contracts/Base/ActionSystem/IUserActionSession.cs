namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionSession : IDisposable
{
    public Task Reset();
    
    public Task<IUserActionResult> ExecuteAction(IUserAction action);
}