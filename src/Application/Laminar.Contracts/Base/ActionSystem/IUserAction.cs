using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserAction
{
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute { get; }

    public Task<IUserActionResult> Execute();
}

public static class UserActionExtensions
{
    public static IObservableValue<bool> CreateCanExecuteObservable(this IUserAction action)
    {
        ObservableValue<bool> output = new(action.CanExecute);
        action.CanExecuteChanged += (_, __) => output.Value = action.CanExecute;
        return output;
    }
}