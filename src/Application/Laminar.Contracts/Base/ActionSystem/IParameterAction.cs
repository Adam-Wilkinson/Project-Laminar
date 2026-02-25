using System.Reflection;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.ActionSystem;

public interface IParameterAction<in TParameter>
{
    public IObservableValue<bool> CanExecute(TParameter parameter);
    
    public IUserAction? Execute(TParameter parameter);
}

public static class ParameterActionExtensions
{
    public static IUserAction WithParameter<TParameter>(this IParameterAction<TParameter> parameterAction, TParameter parameter)
        => new ActionWithParameter<TParameter>(parameterAction, parameter);
    
    private class ActionWithParameter<TParameter> : IUserAction
    {
        private readonly IParameterAction<TParameter> _action;
        private readonly IObservableValue<bool> _canExecute;
        private readonly TParameter _parameter;

        public ActionWithParameter(IParameterAction<TParameter> action, TParameter parameter)
        {
            _action = action;
            _parameter = parameter;
            var canExecuteObservable = _action.CanExecute(_parameter);
            _canExecute = canExecuteObservable;
            canExecuteObservable.ValueChanged += (sender, _) => CanExecuteChanged?.Invoke(sender, EventArgs.Empty);
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute => _canExecute.Value;
        
        public IUserAction Execute() => _action.Execute(_parameter);
    }
}