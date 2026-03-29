using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels.Design;

public class MockUserActionManager : IUserActionManager
{
    public IUserActionResult ExecuteAction(IUserAction action, IActionScope? scope = null)
    {
        return action.CanExecute ? action.Execute() : IUserActionResult.Failure();
    }

    public void Undo(IActionScope? scope = null)
    {
    }

    public void Redo(IActionScope? scope = null)
    {
    }
}