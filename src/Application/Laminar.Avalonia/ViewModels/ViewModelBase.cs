using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.ViewModels.Services;

namespace Laminar.Avalonia.ViewModels;

public abstract partial class ViewModelBase : ObservableObject, IUndoTarget
{
    public IUndoScope? UndoScope { get; set; }

    [RelayCommand]
    public void Undo()
    {
        if (UndoScope is null) return;
    }

    ICommand IUndoTarget.UndoCommand => UndoCommand;
}
