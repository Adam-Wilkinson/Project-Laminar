namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserAction
{
    public bool Execute();

    public IUserAction GetInverse();
}
