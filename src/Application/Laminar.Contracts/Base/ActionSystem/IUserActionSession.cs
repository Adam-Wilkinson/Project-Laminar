namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionSession : IDisposable
{
    public Task Reset();

    public Task Pop();
    
    public Task<IUserActionExecutionOutcome> ExecuteAction(IUserAction action);
}