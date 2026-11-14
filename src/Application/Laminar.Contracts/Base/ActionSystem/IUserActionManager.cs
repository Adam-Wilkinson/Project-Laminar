namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionManager
{
    public IUserActionScope GetScope(string key, params IUserActionSimplifier[] simplifiers);
}
