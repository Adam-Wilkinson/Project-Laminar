using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private readonly UndoCommandClass _undoCommand;

    protected ViewModelBase()
    {
        _undoCommand = new(this);
    }
    
    public ICommand UndoCommand => _undoCommand;

    public IUserActionManager? UserActionManager
    {
        get;
        set
        {
            field = value;
            _undoCommand.UserActionManagerChanged();
        }
    }

    private class UndoCommandClass(ViewModelBase viewModel) : ICommand
    {
        public void UserActionManagerChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        
        public bool CanExecute(object? parameter) => viewModel.UserActionManager is not null;

        public void Execute(object? parameter) => viewModel.UserActionManager?.Undo();

        public event EventHandler? CanExecuteChanged;
    }
}