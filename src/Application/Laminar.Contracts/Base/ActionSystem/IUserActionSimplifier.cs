namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionSimplifier
{
    public IUserActionSimplification Simplify(IUserAction first, IUserAction second);
}