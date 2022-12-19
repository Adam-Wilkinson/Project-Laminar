namespace Laminar.Contracts.ActionSystem;

public interface IUserAction
{
    public bool Execute();

    public IUserAction GetInverse();
}
