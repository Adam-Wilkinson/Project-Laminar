using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    public IUserActionManager? UserActionManager
    {
        get;
        set
        {
            field = value;
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(HasUserActionManager))]
    public Task Undo() => UserActionManager?.Undo() ?? Task.CompletedTask;

    [RelayCommand(CanExecute = nameof(HasUserActionManager))]
    public Task Redo() => UserActionManager?.Redo() ?? Task.CompletedTask;

    public bool HasUserActionManager() => UserActionManager is not null;
}