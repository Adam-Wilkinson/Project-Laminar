using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserAction
{
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute { get; }

    public IUserActionResult Execute();

    public static IUserAction Pass { get; } = new PassAction();

    public class PassAction : IUserAction
    {
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute => false;
        public IUserActionResult Execute()
        {
            return IUserActionResult.Success(this);
        }
    }
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