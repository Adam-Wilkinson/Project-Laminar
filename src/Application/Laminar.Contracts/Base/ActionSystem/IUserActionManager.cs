namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionManager
{
    public IUserActionResult ExecuteAction(IUserAction action);
    
    public void Undo();

    public void Redo();
}
