using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels.Contracts;

public partial class UndoRedoHandler(IUserActionScope scope) : ObservableObject
{
    [RelayCommand]
    private Task<UserActionResult> Redo() => scope.Redo();

    [RelayCommand]
    private Task<UserActionResult> Undo() => scope.Undo();
}

public interface IUndoRedoScope
{
    public UndoRedoHandler UndoRedo { get; }
}