using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels.Services;

public partial class UndoRedoHandler(IUserActionScope scope) : ObservableObject
{
    [RelayCommand]
    private Task Redo() => scope.Redo();

    [RelayCommand]
    private Task Undo() => scope.Undo();
}

public interface IUndoRedoScope
{
    public UndoRedoHandler UndoRedo { get; }
}