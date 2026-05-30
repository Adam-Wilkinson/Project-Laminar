namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserAction
{
    public bool CanExecute { get; }

    public Task<IUserActionResult> Execute();
}