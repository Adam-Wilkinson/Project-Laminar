namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionSessionHost
{
    public void RegisterUndoAction(IUserAction action);

    public void Simplify(List<IUserAction> actions);
}