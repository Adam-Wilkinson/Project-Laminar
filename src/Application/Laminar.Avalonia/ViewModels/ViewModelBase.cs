using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    public IActionScope? UndoScope { get; set; }

    public IUserActionManager? UserActionManager { get; set; }
    
    [RelayCommand]
    private void Undo()
    {
        if (UndoScope is null) return;
        UserActionManager?.Undo(UndoScope);
    }
}