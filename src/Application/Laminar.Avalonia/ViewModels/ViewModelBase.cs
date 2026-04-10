using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private readonly UndoCommandClass _undoCommand;
    private readonly RedoCommandClass _redoCommand;

    protected ViewModelBase()
    {
        _undoCommand = new(this);
        _redoCommand = new(this);
    }

    public ICommand UndoCommand => _undoCommand;

    public ICommand RedoCommand => _redoCommand;

    public IUserActionManager? UserActionManager
    {
        get;
        set
        {
            field = value;
            _undoCommand.UserActionManagerChanged();
            _redoCommand.UserActionManagerChanged();
        }
    }

    private class UndoCommandClass(ViewModelBase viewModel) : ICommand
    {
        public void UserActionManagerChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        
        public bool CanExecute(object? parameter) => viewModel.UserActionManager is not null;

        public void Execute(object? parameter) 
        {
            if (viewModel.UserActionManager?.Undo() is not UserActionSuccess)
            {

            }
        }

        public event EventHandler? CanExecuteChanged;
    }

    private class RedoCommandClass(ViewModelBase viewModel) : ICommand
    {
        public void UserActionManagerChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object? parameter) => viewModel.UserActionManager is not null;

        public void Execute(object? parameter) => viewModel.UserActionManager?.Redo();

        public event EventHandler? CanExecuteChanged;
    }
}