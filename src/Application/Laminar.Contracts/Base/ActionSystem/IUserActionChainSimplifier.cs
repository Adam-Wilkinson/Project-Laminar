namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionChainSimplifier
{
    public bool Simplify(List<IUserAction> chain, ICollection<IUserActionSimplifier> simplifiers);
}