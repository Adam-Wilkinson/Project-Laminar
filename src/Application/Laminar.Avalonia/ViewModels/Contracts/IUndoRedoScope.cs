using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels.Contracts;

public partial class UndoRedoHandler(IUserActionScope scope) : ObservableObject
{
    [RelayCommand]
    private Task<IUserActionResult> Redo() => scope.Redo();

    [RelayCommand]
    private Task<IUserActionResult> Undo() => scope.Undo();
}

public interface IUndoRedoScope
{
    public UndoRedoHandler UndoRedo { get; }
}