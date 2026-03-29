namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionManager
{
    public IUserActionResult ExecuteAction(IUserAction action, IActionScope? scope = null);
    
    public void Undo(IActionScope? scope = null);

    public void Redo(IActionScope? scope = null);
}
