namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserAction
{
    public Task<IUserActionExecutionOutcome> Execute();
}