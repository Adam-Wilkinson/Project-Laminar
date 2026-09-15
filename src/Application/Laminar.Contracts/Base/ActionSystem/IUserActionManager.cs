namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionManager
{
    public IUserActionScope CreateScope(params IUserActionSimplifier[] simplifiers);
}
