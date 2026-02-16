using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels.Design;

public class MockUserActionManager : IUserActionManager
{
    public bool ExecuteAction(IUserAction action, IActionScope? scope = null)
    {
        if (!action.CanExecute) return false;
        
        action.Execute();
        return true;

    }

    public void Undo(IActionScope? scope = null)
    {
    }

    public void Redo(IActionScope? scope = null)
    {
    }
}