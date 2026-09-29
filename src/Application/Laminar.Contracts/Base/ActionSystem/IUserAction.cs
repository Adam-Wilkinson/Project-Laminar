namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserAction
{
    public Task<IUserActionResult> Execute();
}