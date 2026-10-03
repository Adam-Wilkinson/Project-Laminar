namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionResolver
{
    public Task<IUserActionExecutionOutcome> Resolve(IUserAction action);
}